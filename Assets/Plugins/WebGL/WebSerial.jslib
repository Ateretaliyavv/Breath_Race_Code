mergeInto(LibraryManager.library, {
  WebSerial_IsSupported: function () {
    return (typeof navigator !== "undefined" && navigator.serial) ? 1 : 0;
  },

  WebSerial_Connect: function (goPtr, onDataPtr, onStatusPtr) {
    var goName = UTF8ToString(goPtr);
    var onData = UTF8ToString(onDataPtr);
    var onStatus = UTF8ToString(onStatusPtr);

    if (!navigator.serial) {
      try { SendMessage(goName, onStatus, "WebSerial not supported (use Chrome/Edge)."); } catch (e) {}
      return;
    }

    // Use window as a stable global store across scene loads.
    if (!window.__webSerialUnity) window.__webSerialUnity = {};
    var S = window.__webSerialUnity;

    // Update receiver target every time (scene can change).
    S.goName = goName;
    S.onData = onData;
    S.onStatus = onStatus;

    // Optional hard fallback receiver name (use ONLY if you guarantee it exists).
    // If you use DontDestroyOnLoad and keep the receiver object named "BreathUSB",
    // this helps when Unity temporarily unloads objects during scene switches.
    S.fallbackGoName = "BreathUSB";

    // Safe SendMessage:
    // - Try current receiver
    // - If missing, try fallback receiver
    // - Never throw (keep the serial loop alive)
    function safeSend(method, message) {
      try {
        SendMessage(S.goName, method, message);
        return;
      } catch (e1) {}

      try {
        SendMessage(S.fallbackGoName, method, message);
      } catch (e2) {}
    }

    (async function () {
      try {
        // Prevent double-connect: close previous session if exists.
        if (S.port || S.reader) {
          S.keepReading = false;

          if (S.reader) {
            try { await S.reader.cancel(); } catch (e) {}
            try { S.reader.releaseLock(); } catch (e) {}
            S.reader = null;
          }

          if (S.port) {
            try { await S.port.close(); } catch (e) {}
            S.port = null;
          }
        }

        S.keepReading = true;

        safeSend(S.onStatus, "Requesting serial port...");

        // Must be called from a user gesture (button click).
        S.port = await navigator.serial.requestPort();

        // Match ESP32 Serial.begin(115200).
        await S.port.open({ baudRate: 115200 });

        safeSend(S.onStatus, "Serial connected (115200).");

        // Read loop: accumulate chunks and split by newline.
        const decoder = new TextDecoder();
        S.reader = S.port.readable.getReader();

        let buffer = "";

        while (S.keepReading) {
          const { value, done } = await S.reader.read();
          if (done) break;
          if (!value) continue;

          buffer += decoder.decode(value, { stream: true });

          let idx;
          while ((idx = buffer.indexOf("\n")) >= 0) {
            let line = buffer.slice(0, idx).trim();
            buffer = buffer.slice(idx + 1);

            if (line.length > 0) {
              safeSend(S.onData, line);
            }
          }
        }

        safeSend(S.onStatus, "Serial read loop stopped.");
      } catch (e) {
        safeSend(S.onStatus, "Serial error: " + (e && e.message ? e.message : e));
      }
    })();
  },

  WebSerial_Disconnect: function () {
    (async function () {
      try {
        if (!window.__webSerialUnity) return;
        var S = window.__webSerialUnity;

        S.keepReading = false;

        if (S.reader) {
          try { await S.reader.cancel(); } catch (e) {}
          try { S.reader.releaseLock(); } catch (e) {}
          S.reader = null;
        }

        if (S.port) {
          try { await S.port.close(); } catch (e) {}
          S.port = null;
        }
      } catch (e) {}
    })();
  }
});
