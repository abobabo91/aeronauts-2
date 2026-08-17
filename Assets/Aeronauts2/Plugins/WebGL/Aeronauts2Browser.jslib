mergeInto(LibraryManager.library, {
  Aeronauts2_ReportTelemetry: function (jsonPointer) {
    try {
      window.__aeronauts2Telemetry = JSON.parse(UTF8ToString(jsonPointer));
    } catch (error) {
      console.error("Aeronauts 2 telemetry bridge failed", error);
    }
  },
});
