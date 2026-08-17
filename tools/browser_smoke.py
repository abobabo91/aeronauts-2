"""Exercise the real Aeronauts 2 WebGL build through an existing CDP Chrome."""

from __future__ import annotations

import argparse
import json
import math
import pathlib
import statistics
import time

from playwright.sync_api import (
    ConsoleMessage,
    Page,
    Request,
    TimeoutError as PlaywrightTimeoutError,
    sync_playwright,
)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--cdp", default="http://127.0.0.1:9230")
    parser.add_argument("--url", default="http://127.0.0.1:8008/")
    parser.add_argument("--output-dir", default="Logs/browser")
    return parser.parse_args()


def percentile(values: list[float], fraction: float) -> float:
    if not values:
        return 0.0
    ordered = sorted(values)
    index = min(len(ordered) - 1, math.ceil(len(ordered) * fraction) - 1)
    return ordered[index]


def capture_frame_timing(page: Page, frame_count: int = 180) -> dict[str, float]:
    intervals = page.evaluate(
        """async (frameCount) => {
            const samples = [];
            let previous = performance.now();
            for (let index = 0; index < frameCount; index++) {
                const now = await new Promise(resolve => requestAnimationFrame(resolve));
                samples.push(now - previous);
                previous = now;
            }
            return samples;
        }""",
        frame_count,
    )
    usable = [float(value) for value in intervals[5:] if 0 < value < 1000]
    average_ms = statistics.fmean(usable) if usable else 0.0
    return {
        "sample_count": len(usable),
        "average_frame_ms": round(average_ms, 3),
        "p95_frame_ms": round(percentile(usable, 0.95), 3),
        "estimated_fps": round(1000.0 / average_ms, 1) if average_ms else 0.0,
    }


def main() -> int:
    args = parse_args()
    output_dir = pathlib.Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    console_messages: list[dict[str, str]] = []
    page_errors: list[str] = []
    failed_requests: list[dict[str, str]] = []

    with sync_playwright() as playwright:
        browser = playwright.chromium.connect_over_cdp(args.cdp)
        context = browser.contexts[0]
        page = context.new_page()

        def on_console(message: ConsoleMessage) -> None:
            console_messages.append({"type": message.type, "text": message.text})

        def on_request_failed(request: Request) -> None:
            if "favicon" not in request.url:
                failed_requests.append(
                    {"url": request.url, "failure": request.failure or "unknown"}
                )

        page.on("console", on_console)
        page.on("pageerror", lambda error: page_errors.append(str(error)))
        page.on("requestfailed", on_request_failed)

        started = time.perf_counter()
        page.goto(args.url, wait_until="domcontentloaded", timeout=120_000)
        page.wait_for_selector("#unity-canvas", state="visible", timeout=120_000)
        page.wait_for_function(
            """() => {
                const bar = document.querySelector('#unity-loading-bar');
                return !bar || getComputedStyle(bar).display === 'none';
            }""",
            timeout=180_000,
        )
        page.wait_for_function(
            "() => window.__aeronauts2Telemetry !== undefined", timeout=30_000
        )
        page.wait_for_timeout(2_000)
        load_seconds = time.perf_counter() - started

        canvas = page.locator("#unity-canvas")
        canvas_box = canvas.bounding_box()
        if canvas_box is None:
            raise RuntimeError("Unity canvas has no rendered bounding box.")

        webgl = page.evaluate(
            """() => {
                const canvas = document.querySelector('#unity-canvas');
                const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');
                if (!gl) return {available: false};
                const extension = gl.getExtension('WEBGL_debug_renderer_info');
                return {
                    available: true,
                    version: gl.getParameter(gl.VERSION),
                    vendor: extension ? gl.getParameter(extension.UNMASKED_VENDOR_WEBGL) : null,
                    renderer: extension ? gl.getParameter(extension.UNMASKED_RENDERER_WEBGL) : null,
                    drawingBufferWidth: gl.drawingBufferWidth,
                    drawingBufferHeight: gl.drawingBufferHeight,
                };
            }"""
        )
        page.screenshot(path=str(output_dir / "01-loaded.png"))

        center_x = canvas_box["x"] + canvas_box["width"] / 2
        center_y = canvas_box["y"] + canvas_box["height"] / 2
        page.mouse.click(center_x, center_y)
        page.wait_for_timeout(300)
        pointer_locked = page.evaluate(
            "document.pointerLockElement === document.querySelector('#unity-canvas')"
        )

        initial_aim_telemetry = page.evaluate("window.__aeronauts2Telemetry")
        page.keyboard.press("v")
        try:
            page.wait_for_function(
                """() => window.__aeronauts2Telemetry
                    && window.__aeronauts2Telemetry.aimLimitMode === 'Full360'""",
                timeout=2_000,
            )
        except PlaywrightTimeoutError:
            pass
        full360_aim_telemetry = page.evaluate("window.__aeronauts2Telemetry")
        page.keyboard.press("v")
        try:
            page.wait_for_function(
                """() => window.__aeronauts2Telemetry
                    && window.__aeronauts2Telemetry.aimLimitMode === 'Constrained'""",
                timeout=2_000,
            )
        except PlaywrightTimeoutError:
            pass
        constrained_aim_telemetry = page.evaluate("window.__aeronauts2Telemetry")

        page.keyboard.down("Space")
        page.keyboard.down("w")
        page.wait_for_timeout(1_200)
        page.keyboard.up("w")
        page.keyboard.up("Space")
        page.keyboard.down("q")
        page.wait_for_timeout(300)
        page.keyboard.up("q")
        page.keyboard.press("e")
        page.wait_for_timeout(90)
        page.keyboard.press("e")
        page.wait_for_timeout(700)
        flight_telemetry = page.evaluate("window.__aeronauts2Telemetry")
        page.screenshot(path=str(output_dir / "02-flight.png"))

        page.keyboard.press("c")
        try:
            page.wait_for_function(
                """() => {
                    const telemetry = window.__aeronauts2Telemetry;
                    return telemetry
                        && telemetry.cameraViewMode === 'Forward'
                        && telemetry.aimBodyAngle < 1;
                }""",
                timeout=2_000,
            )
        except PlaywrightTimeoutError:
            pass
        forward_view_telemetry = page.evaluate("window.__aeronauts2Telemetry")
        page.screenshot(path=str(output_dir / "03-forward.png"))
        page.keyboard.press("c")
        try:
            page.wait_for_function(
                """() => {
                    const telemetry = window.__aeronauts2Telemetry;
                    return telemetry
                        && telemetry.cameraViewMode === 'Aim'
                        && telemetry.aimBodyAngle < 1;
                }""",
                timeout=2_000,
            )
        except PlaywrightTimeoutError:
            pass
        returned_aim_view_telemetry = page.evaluate("window.__aeronauts2Telemetry")

        page.keyboard.press("r")
        page.wait_for_timeout(400)
        page.mouse.down(button="left")
        page.wait_for_timeout(500)
        page.mouse.up(button="left")
        page.wait_for_timeout(1_000)
        page.mouse.click(center_x, center_y, button="right")
        try:
            page.wait_for_function(
                """() => {
                    const telemetry = window.__aeronauts2Telemetry;
                    return telemetry
                        && telemetry.railgunShots === 1
                        && telemetry.railgunAmmo === 2;
                }""",
                timeout=2_000,
            )
        except PlaywrightTimeoutError:
            # Keep collecting the report so the behavior assertions below explain
            # a genuine input failure instead of ending without diagnostics.
            pass
        combat_telemetry = page.evaluate("window.__aeronauts2Telemetry")
        page.screenshot(path=str(output_dir / "04-combat.png"))

        frame_timing = capture_frame_timing(page)
        canvas_state = page.evaluate(
            """() => {
                const canvas = document.querySelector('#unity-canvas');
                const rect = canvas.getBoundingClientRect();
                return {
                    cssWidth: rect.width,
                    cssHeight: rect.height,
                    pixelWidth: canvas.width,
                    pixelHeight: canvas.height,
                    visibility: document.visibilityState,
                };
            }"""
        )

        known_shader_fragments = (
            "Hidden/CoreSRP/CoreCopy shader is not supported",
            "Hidden/Universal/HDRDebugView shader is not supported",
        )
        known_shader_warnings: list[str] = []
        unity_runtime_errors: list[str] = []
        for index, message in enumerate(console_messages):
            text = message["text"].strip()
            if any(fragment in text for fragment in known_shader_fragments):
                known_shader_warnings.append(text)
                continue
            if text == "ERROR: Shader":
                following = (
                    console_messages[index + 1]["text"]
                    if index + 1 < len(console_messages)
                    else ""
                )
                if any(fragment in following for fragment in known_shader_fragments):
                    continue
            if "ERROR:" in text:
                unity_runtime_errors.append(text)

        error_console = [
            message
            for message in console_messages
            if message["type"] in {"error", "assert"}
            and "favicon" not in message["text"].lower()
        ]
        unity_cache_succeeded = any(
            "[UnityCache]" in message["text"]
            and (
                "successfully downloaded" in message["text"]
                or "successfully revalidated" in message["text"]
            )
            for message in console_messages
        )
        expected_cache_aborts = [
            request
            for request in failed_requests
            if request["failure"] == "net::ERR_ABORTED"
            and request["url"].endswith(".data.br")
            and unity_cache_succeeded
        ]
        failed_requests = [
            request for request in failed_requests if request not in expected_cache_aborts
        ]
        behavior_failures: list[str] = []
        if float(flight_telemetry.get("speed", 0)) <= 1:
            behavior_failures.append("Keyboard flight input did not produce forward speed.")
        if initial_aim_telemetry.get("cameraViewMode") != "Aim":
            behavior_failures.append("Camera did not start in aim view.")
        if initial_aim_telemetry.get("aimLimitMode") != "Constrained":
            behavior_failures.append("Aim limits did not start in constrained mode.")
        if full360_aim_telemetry.get("aimLimitMode") != "Full360":
            behavior_failures.append("Aim-limit toggle did not enter full-360 mode.")
        if float(full360_aim_telemetry.get("aimHorizontalLimit", 0)) != 180:
            behavior_failures.append("Full-360 mode did not expose a 180-degree per-side horizontal limit.")
        if constrained_aim_telemetry.get("aimLimitMode") != "Constrained":
            behavior_failures.append("Aim-limit toggle did not return to constrained mode.")
        if float(constrained_aim_telemetry.get("aimHorizontalLimit", 0)) != 110:
            behavior_failures.append("Constrained mode did not restore the 110-degree per-side horizontal limit.")
        if float(flight_telemetry.get("aimBodyAngle", 0)) <= 5:
            behavior_failures.append("Flight sequence did not create an independent off-body aim angle.")
        if forward_view_telemetry.get("cameraViewMode") != "Forward":
            behavior_failures.append("View toggle did not enter forward view.")
        if float(forward_view_telemetry.get("aimBodyAngle", 180)) >= 1:
            behavior_failures.append("Forward view did not align camera and weapons with the body.")
        if returned_aim_view_telemetry.get("cameraViewMode") != "Aim":
            behavior_failures.append("View toggle did not return to aim view.")
        if float(returned_aim_view_telemetry.get("aimBodyAngle", 180)) >= 1:
            behavior_failures.append("Returning to aim view did not begin from body forward.")
        if int(combat_telemetry.get("primaryShots", 0)) < 5:
            behavior_failures.append("Primary mouse input did not produce five shots.")
        if int(combat_telemetry.get("railgunShots", 0)) != 1:
            behavior_failures.append("Secondary mouse input did not produce one railgun shot.")
        if int(combat_telemetry.get("hits", 0)) < 1:
            behavior_failures.append("Scripted centered weapon fire registered no hits.")
        if int(combat_telemetry.get("railgunAmmo", -1)) != 2:
            behavior_failures.append("Railgun reserve did not decrease from three to two.")
        if bool(combat_telemetry.get("muzzleBlocked", True)):
            behavior_failures.append("Clear centered combat incorrectly reported the head origin as blocked.")
        if bool(combat_telemetry.get("lastShotWasBlocked", True)):
            behavior_failures.append("Clear railgun shot was incorrectly classified as head-origin blocked.")
        report = {
            "url": page.url,
            "title": page.title(),
            "load_seconds": round(load_seconds, 3),
            "pointer_locked": pointer_locked,
            "webgl": webgl,
            "canvas": canvas_state,
            "frame_timing": frame_timing,
            "initial_aim_telemetry": initial_aim_telemetry,
            "full360_aim_telemetry": full360_aim_telemetry,
            "constrained_aim_telemetry": constrained_aim_telemetry,
            "flight_telemetry": flight_telemetry,
            "forward_view_telemetry": forward_view_telemetry,
            "returned_aim_view_telemetry": returned_aim_view_telemetry,
            "combat_telemetry": combat_telemetry,
            "behavior_failures": behavior_failures,
            "console_messages": console_messages,
            "page_errors": page_errors,
            "failed_requests": failed_requests,
            "expected_cache_aborts": expected_cache_aborts,
            "error_console": error_console,
            "unity_runtime_errors": unity_runtime_errors,
            "known_shader_warnings": known_shader_warnings,
        }
        (output_dir / "report.json").write_text(
            json.dumps(report, indent=2), encoding="utf-8"
        )
        print(json.dumps(report, indent=2))
        page.close()

    return (
        1
        if page_errors
        or failed_requests
        or error_console
        or unity_runtime_errors
        or behavior_failures
        or not webgl["available"]
        else 0
    )


if __name__ == "__main__":
    raise SystemExit(main())
