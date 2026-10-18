(() => {
  "use strict";
  const $ = (id) => document.getElementById(id);
  const els = {
    cameraSelect: $("cameraSelect"), cameraButton: $("cameraButton"), cameraStatus: $("cameraStatus"), cameraLamp: $("cameraLamp"),
    screenButton: $("screenButton"), screenStatus: $("screenStatus"), screenLamp: $("screenLamp"), screenAudio: $("screenAudio"),
    micSelect: $("micSelect"), micStatus: $("micStatus"), micGain: $("micGain"), micGainValue: $("micGainValue"), micMeter: $("micMeter"), micLevelLabel: $("micLevelLabel"),
    canvas: $("programCanvas"), cameraVideo: $("cameraVideo"), screenVideo: $("screenVideo"), placeholder: $("canvasPlaceholder"),
    liveDot: $("liveDot"), topStatus: $("topStatus"), previewStatus: $("previewStatus"), frameRateStatus: $("frameRateStatus"),
    recIndicator: $("recIndicator"), resolutionBadge: $("resolutionBadge"), sceneButtons: [...document.querySelectorAll("[data-scene]")],
    showTitle: $("showTitle"), showSubtitle: $("showSubtitle"), overlayToggle: $("overlayToggle"), resolutionSelect: $("resolutionSelect"), fpsSelect: $("fpsSelect"), transitionSelect: $("transitionSelect"),
    snapshotButton: $("snapshotButton"), snapshotStatus: $("snapshotStatus"), recordButton: $("recordButton"), stopButton: $("stopButton"),
    recordingHeadline: $("recordingHeadline"), recordingHelp: $("recordingHelp"), recordTime: $("recordTime"),
    saveProfileButton: $("saveProfileButton"), exportProfileButton: $("exportProfileButton"), importProfileButton: $("importProfileButton"), profileFile: $("profileFile"), profileStatus: $("profileStatus")
  };
  const ctx = els.canvas.getContext("2d", { alpha: false });
  const state = { cameraStream: null, screenStream: null, micStream: null, recorder: null, chunks: [], scene: "camera", raf: 0, startedAt: 0, timer: 0, audioContext: null, audioDestination: null, micSource: null, screenSource: null, screenGain: null, analyser: null, gain: null, meterFrame: 0, recordingUrl: null, fadeUntil: 0, profileKey: "jt-stream-profile-v1" };

  function note(message) {
    els.topStatus.textContent = message;
    els.previewStatus.textContent = message;
  }
  function secureCaptureAvailable() {
    return !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia && navigator.mediaDevices.getDisplayMedia);
  }
  function setLamp(el, active) { if (el) el.classList.toggle("active", !!active); }
  function liveTracks(stream) { return !!stream && stream.getVideoTracks().some(t => t.readyState === "live"); }
  function waitForVideo(video) {
    return new Promise((resolve, reject) => {
      if (video.readyState >= 2) return resolve();
      const timeout = setTimeout(() => reject(new Error("The source did not become ready. Check the selected device and try again.")), 10000);
      video.addEventListener("loadeddata", () => { clearTimeout(timeout); resolve(); }, { once: true });
      video.addEventListener("error", () => { clearTimeout(timeout); reject(new Error("The browser could not read this media source.")); }, { once: true });
    });
  }
  async function refreshDevices() {
    if (!navigator.mediaDevices?.enumerateDevices) return;
    try {
      const devices = await navigator.mediaDevices.enumerateDevices();
      const fill = (select, kind, label) => {
        const previous = select.value;
        select.replaceChildren(new Option(label, ""));
        devices.filter(d => d.kind === kind).forEach((d, i) => select.add(new Option(d.label || (kind === "videoinput" ? "Camera " : "Microphone ") + (i + 1), d.deviceId)));
        if ([...select.options].some(o => o.value === previous)) select.value = previous;
      };
      fill(els.cameraSelect, "videoinput", "Default camera");
      fill(els.micSelect, "audioinput", "Default microphone");
    } catch (error) { console.warn("Could not enumerate media devices", error); }
  }
  async function enableCamera() {
    if (state.cameraStream) {
      stopCamera();
      els.cameraButton.textContent = "Enable camera + microphone";
      note(liveTracks(state.screenStream) ? "Screen ready · camera stopped" : "Studio ready · no source");
      draw();
      return;
    }
    if (!secureCaptureAvailable()) {
      note("Camera access needs a supported browser on HTTPS or localhost.");
      alert("JT Stream needs a supported browser on HTTPS or localhost to access camera and microphone devices.");
      return;
    }
    els.cameraButton.disabled = true;
    try {
      stopCamera();
      const video = els.cameraSelect.value ? { deviceId: { exact: els.cameraSelect.value }, width: { ideal: 1280 }, height: { ideal: 720 } } : { width: { ideal: 1280 }, height: { ideal: 720 } };
      const audio = els.micSelect.value ? { deviceId: { exact: els.micSelect.value }, echoCancellation: true, noiseSuppression: true } : { echoCancellation: true, noiseSuppression: true };
      const stream = await navigator.mediaDevices.getUserMedia({ video, audio });
      state.cameraStream = stream;
      els.cameraVideo.srcObject = stream;
      await els.cameraVideo.play();
      await waitForVideo(els.cameraVideo);
      els.cameraStatus.textContent = "Connected";
      els.micStatus.textContent = stream.getAudioTracks().length ? "Connected" : "No audio track";
      els.cameraButton.textContent = "Turn camera + mic off";
      setLamp(els.cameraLamp, true);
      await refreshDevices();
      if (stream.getAudioTracks().length) await setupAudio(stream);
      els.placeholder.hidden = true;
      els.recordButton.disabled = false;
      note("Camera ready · not broadcasting");
      draw();
    } catch (error) {
      note(error.name === "NotAllowedError" ? "Permission denied — allow camera and microphone access in your browser." : "Could not start the camera.");
      alert("JT Stream could not start the camera: " + (error.message || error.name) + "\n\nCheck browser permissions and make sure another app is not exclusively using the device.");
    } finally { els.cameraButton.disabled = false; }
  }
  function stopCamera() {
    if (state.cameraStream) state.cameraStream.getTracks().forEach(t => t.stop());
    state.cameraStream = null;
    els.cameraVideo.srcObject = null;
    els.cameraStatus.textContent = "Not connected";
    els.micStatus.textContent = "Not connected";
    els.cameraButton.textContent = "Enable camera + microphone";
    setLamp(els.cameraLamp, false);
    stopAudioMeter();
    if (!liveTracks(state.screenStream)) els.recordButton.disabled = true;
  }
  async function shareScreen() {
    if (state.screenStream) {
      stopScreen();
      els.screenButton.textContent = "Share screen or window";
      draw();
      return;
    }
    if (!navigator.mediaDevices?.getDisplayMedia) {
      alert("Screen sharing is not available in this browser or context. Use a supported browser on HTTPS or localhost.");
      return;
    }
    try {
      const stream = await navigator.mediaDevices.getDisplayMedia({ video: { frameRate: { ideal: Number(els.fpsSelect.value) || 30 } }, audio: els.screenAudio.checked });
      if (state.screenStream) state.screenStream.getTracks().forEach(t => t.stop());
      state.screenStream = stream;
      els.screenVideo.srcObject = stream;
      await els.screenVideo.play();
      await waitForVideo(els.screenVideo);
      const videoTrack = stream.getVideoTracks()[0];
      if (videoTrack) videoTrack.addEventListener("ended", () => stopScreen(), { once: true });
      els.screenStatus.textContent = "Sharing";
      els.screenButton.textContent = "Stop screen sharing";
      setLamp(els.screenLamp, true);
      els.placeholder.hidden = true;
      els.recordButton.disabled = false;
      if (stream.getAudioTracks().length) {
        await setupScreenAudio(stream);
        note("Screen shared with available tab audio · local preview only");
      } else note("Screen shared · local preview only");
      draw();
    } catch (error) {
      if (error.name !== "NotAllowedError") alert("Screen sharing could not start: " + (error.message || error.name));
      note(error.name === "NotAllowedError" ? "Screen sharing cancelled." : "Screen sharing unavailable.");
    }
  }
  function stopScreen() {
    if (state.screenStream) state.screenStream.getTracks().forEach(t => t.stop());
    state.screenStream = null;
    els.screenVideo.srcObject = null;
    if (state.screenSource) { try { state.screenSource.disconnect(); } catch (_) {} state.screenSource = null; }
    if (state.screenGain) { try { state.screenGain.disconnect(); } catch (_) {} state.screenGain = null; }
    els.screenStatus.textContent = "Not sharing";
    els.screenButton.textContent = "Share screen or window";
    setLamp(els.screenLamp, false);
    if (!liveTracks(state.cameraStream)) els.recordButton.disabled = true;
    if (!state.gain) closeAudioGraphIfUnused();
    note(liveTracks(state.cameraStream) ? "Camera ready · screen stopped" : "Studio ready · no source");
  }
  async function ensureAudioGraph() {
    const AudioContextClass = window.AudioContext || window.webkitAudioContext;
    if (!AudioContextClass) throw new Error("Audio mixing is unavailable in this browser.");
    if (!state.audioContext || state.audioContext.state === "closed") {
      state.audioContext = new AudioContextClass();
      state.audioDestination = state.audioContext.createMediaStreamDestination();
    }
    if (state.audioContext.state === "suspended") await state.audioContext.resume();
  }
  async function setupAudio(stream) {
    stopAudioMeter();
    const AudioContextClass = window.AudioContext || window.webkitAudioContext;
    if (!AudioContextClass) { els.micLevelLabel.textContent = "Meter unsupported"; return; }
    try {
      await ensureAudioGraph();
      state.micSource = state.audioContext.createMediaStreamSource(new MediaStream(stream.getAudioTracks()));
      state.gain = state.audioContext.createGain();
      state.gain.gain.value = Number(els.micGain.value) / 100;
      state.analyser = state.audioContext.createAnalyser();
      state.analyser.fftSize = 256;
      state.micSource.connect(state.gain);
      state.gain.connect(state.analyser);
      state.gain.connect(state.audioDestination);
      const samples = new Uint8Array(state.analyser.fftSize);
      const tick = () => {
        if (!state.analyser) return;
        state.analyser.getByteTimeDomainData(samples);
        let sum = 0;
        for (const v of samples) { const n = (v - 128) / 128; sum += n * n; }
        const rms = Math.sqrt(sum / samples.length);
        const level = Math.min(100, Math.round(rms * 250));
        els.micMeter.style.width = level + "%";
        els.micLevelLabel.textContent = level > 2 ? level + "% signal" : "Waiting for input";
        state.meterFrame = requestAnimationFrame(tick);
      };
      tick();
    } catch (error) { console.warn("Audio meter could not start", error); els.micLevelLabel.textContent = "Meter unavailable"; }
  }
  async function setupScreenAudio(stream) {
    if (!stream.getAudioTracks().length) return;
    await ensureAudioGraph();
    if (state.screenSource) { try { state.screenSource.disconnect(); } catch (_) {} }
    if (state.screenGain) { try { state.screenGain.disconnect(); } catch (_) {} }
    state.screenSource = state.audioContext.createMediaStreamSource(new MediaStream(stream.getAudioTracks()));
    state.screenGain = state.audioContext.createGain();
    state.screenGain.gain.value = 1;
    state.screenSource.connect(state.screenGain);
    state.screenGain.connect(state.audioDestination);
  }
  function stopAudioMeter() {
    if (state.meterFrame) cancelAnimationFrame(state.meterFrame);
    state.meterFrame = 0;
    if (state.micSource) { try { state.micSource.disconnect(); } catch (_) {} }
    if (state.gain) { try { state.gain.disconnect(); } catch (_) {} }
    state.micSource = null; state.analyser = null; state.gain = null;
    els.micMeter.style.width = "0%"; els.micLevelLabel.textContent = "Waiting for input";
    closeAudioGraphIfUnused();
  }
  function closeAudioGraphIfUnused() {
    if (state.gain || state.screenSource) return;
    if (state.audioContext) state.audioContext.close().catch(() => {});
    state.audioContext = null; state.audioDestination = null; state.analyser = null;
  }
  function fitVideo(video, x, y, w, h) {
    if (!video || video.readyState < 2 || !video.videoWidth || !video.videoHeight) return;
    const scale = Math.max(w / video.videoWidth, h / video.videoHeight);
    const sw = w / scale, sh = h / scale;
    const sx = (video.videoWidth - sw) / 2, sy = (video.videoHeight - sh) / 2;
    ctx.save();
    if (video === els.cameraVideo && els.mirrorToggle?.checked) { ctx.translate(x + w, y); ctx.scale(-1, 1); ctx.drawImage(video, sx, sy, sw, sh, 0, 0, w, h); }
    else ctx.drawImage(video, sx, sy, sw, sh, x, y, w, h);
    ctx.restore();
  }
  function draw() {
    if (state.raf) cancelAnimationFrame(state.raf);
    const wide = Number(els.resolutionSelect.value) === 1080;
    const width = wide ? 1920 : 1280, height = wide ? 1080 : 720;
    if (els.canvas.width !== width || els.canvas.height !== height) { els.canvas.width = width; els.canvas.height = height; els.resolutionBadge.textContent = width + " × " + height; }
    ctx.fillStyle = "#090a12"; ctx.fillRect(0, 0, width, height);
    const cam = liveTracks(state.cameraStream) && els.cameraVideo.readyState >= 2;
    const screen = liveTracks(state.screenStream) && els.screenVideo.readyState >= 2;
    if (state.scene === "screen" && screen) fitVideo(els.screenVideo, 0, 0, width, height);
    else if (state.scene === "pip" && screen) {
      fitVideo(els.screenVideo, 0, 0, width, height);
      if (cam) {
        const pw = Math.round(width * 0.27), ph = Math.round(pw * 9 / 16), pad = Math.round(width * 0.025);
        ctx.save(); ctx.shadowColor = "#0009"; ctx.shadowBlur = 24; ctx.fillStyle = "#0a0b14"; ctx.fillRect(width-pw-pad-7,height-ph-pad-7,pw+14,ph+14); ctx.restore();
        fitVideo(els.cameraVideo, width-pw-pad, height-ph-pad, pw, ph);
      }
    } else if (cam) fitVideo(els.cameraVideo, 0, 0, width, height);
    else if (screen) fitVideo(els.screenVideo, 0, 0, width, height);
    else {
      const gradient = ctx.createLinearGradient(0, 0, width, height); gradient.addColorStop(0, "#171329"); gradient.addColorStop(1, "#090a12"); ctx.fillStyle = gradient; ctx.fillRect(0, 0, width, height);
      ctx.fillStyle = "#ff8ab6"; ctx.font = "800 " + Math.round(width*.045) + "px system-ui"; ctx.fillText("JT", width*.06, height*.17);
      ctx.fillStyle = "#f4f4fb"; ctx.font = "700 " + Math.round(width*.035) + "px system-ui"; ctx.fillText("Your studio, ready when you are.", width*.06, height*.27);
      ctx.fillStyle = "#a5a5bd"; ctx.font = Math.round(width*.018) + "px system-ui"; ctx.fillText("Enable a camera or share a screen to start.", width*.06, height*.32);
    }
    if (state.fadeUntil > 0) {
      const remaining = state.fadeUntil - performance.now();
      if (remaining <= 0) state.fadeUntil = 0;
      else { ctx.save(); ctx.globalAlpha = Math.min(1, remaining / 130); ctx.fillStyle = "#000"; ctx.fillRect(0, 0, width, height); ctx.restore(); }
    }
    if (els.overlayToggle.checked) {
      const barH = Math.round(height * .13), pad = Math.round(width * .035), y = height - barH - pad;
      ctx.fillStyle = "rgba(8,9,17,.82)"; ctx.fillRect(pad, y, width - pad*2, barH);
      ctx.fillStyle = "#ff8ab6"; ctx.fillRect(pad, y, Math.max(6, Math.round(width*.008)), barH);
      ctx.fillStyle = "#ff8ab6"; ctx.font = "800 " + Math.round(height*.03) + "px system-ui"; ctx.fillText("JT", pad*1.8, y + barH*.40);
      ctx.fillStyle = "#f8f7ff"; ctx.font = "700 " + Math.round(height*.031) + "px system-ui";
      ctx.fillText((els.showTitle.value || "JT Stream").slice(0, 90), pad*3.2, y + barH*.42, width-pad*5);
      ctx.fillStyle = "#c1c0d2"; ctx.font = Math.round(height*.018) + "px system-ui";
      ctx.fillText((els.showSubtitle.value || "").slice(0,120), pad*3.2, y + barH*.72, width-pad*5);
    }
    els.placeholder.hidden = cam || screen;
    els.frameRateStatus.textContent = els.fpsSelect.value + " FPS target";
    state.raf = requestAnimationFrame(draw);
  }
  function setScene(scene) {
    state.scene = scene;
    if (els.transitionSelect?.value === "fade") state.fadeUntil = performance.now() + 260;
    els.sceneButtons.forEach(b => { const active = b.dataset.scene === scene; b.classList.toggle("selected", active); b.setAttribute("aria-pressed", String(active)); });
    note("Scene changed · " + ({camera:"Camera",screen:"Screen",pip:"Picture-in-picture"}[scene] || scene) + (els.transitionSelect?.value === "fade" ? " · fade transition" : " · cut"));
  }
  function profile() {
    return { version: 1, scene: state.scene, transition: els.transitionSelect?.value || "cut", title: els.showTitle.value, subtitle: els.showSubtitle.value, overlay: els.overlayToggle.checked, resolution: els.resolutionSelect.value, fps: els.fpsSelect.value, micGain: els.micGain.value, screenAudio: els.screenAudio.checked, savedAt: new Date().toISOString() };
  }
  function applyProfile(p) {
    if (!p || p.version !== 1) throw new Error("Unsupported JT Stream profile format.");
    if (els.transitionSelect) els.transitionSelect.value = p.transition === "fade" ? "fade" : "cut";
    setScene(["camera","screen","pip"].includes(p.scene) ? p.scene : "camera");
    els.showTitle.value = String(p.title || "").slice(0,90);
    els.showSubtitle.value = String(p.subtitle || "").slice(0,120);
    els.overlayToggle.checked = p.overlay !== false;
    els.resolutionSelect.value = p.resolution === "1080" ? "1080" : "720";
    els.fpsSelect.value = ["24","30","60"].includes(String(p.fps)) ? String(p.fps) : "30";
    els.micGain.value = String(Math.max(0,Math.min(200,Number(p.micGain ?? 100))));
    els.screenAudio.checked = !!p.screenAudio;
    if (state.gain) state.gain.gain.value = Number(els.micGain.value) / 100;
    draw();
  }
  function downloadBlob(blob, name) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a"); a.href = url; a.download = name; a.click();
    setTimeout(() => URL.revokeObjectURL(url), 30000);
  }
  function takeSnapshot() {
    els.canvas.toBlob(blob => {
      if (!blob) { els.snapshotStatus.textContent = "Snapshot failed"; return; }
      downloadBlob(blob, "jt-stream-snapshot-" + new Date().toISOString().replace(/[:.]/g,"-") + ".png");
      els.snapshotStatus.textContent = "PNG saved";
      setTimeout(() => els.snapshotStatus.textContent = "PNG snapshot", 3500);
    }, "image/png");
  }
  function startRecording() {
    if (!("MediaRecorder" in window) || !els.canvas.captureStream) {
      alert("This browser does not support canvas recording. Try a current Chrome, Edge, or Firefox release.");
      return;
    }
    const hasSource = liveTracks(state.cameraStream) || liveTracks(state.screenStream);
    if (!hasSource) { alert("Enable a camera or share your screen before recording."); return; }
    const fps = Number(els.fpsSelect.value) || 30;
    const canvasStream = els.canvas.captureStream(fps);
    const output = new MediaStream(canvasStream.getVideoTracks());
    if (state.audioDestination) state.audioDestination.stream.getAudioTracks().forEach(track => output.addTrack(track));
    const mimeType = ["video/webm;codecs=vp9,opus","video/webm;codecs=vp8,opus","video/webm"].find(type => MediaRecorder.isTypeSupported(type));
    try {
      state.chunks = [];
      state.recorder = new MediaRecorder(output, mimeType ? { mimeType } : undefined);
      state.recorder.ondataavailable = e => { if (e.data && e.data.size) state.chunks.push(e.data); };
      state.recorder.onerror = e => { note("Recording error: " + (e.error?.message || "unknown")); };
      state.recorder.onstop = () => {
        const blob = new Blob(state.chunks, { type: state.recorder.mimeType || "video/webm" });
        if (state.recordingUrl) URL.revokeObjectURL(state.recordingUrl);
        state.recordingUrl = URL.createObjectURL(blob);
        let link = document.getElementById("jtStreamRecordingDownload");
        if (!link) { link = document.createElement("a"); link.id = "jtStreamRecordingDownload"; link.className = "wide-button"; link.style.marginTop = "12px"; els.recordingHelp.insertAdjacentElement("afterend", link); }
        link.href = state.recordingUrl; link.download = "jt-stream-recording-" + new Date().toISOString().replace(/[:.]/g,"-") + ".webm"; link.textContent = "Download recording (" + Math.max(1,Math.round(blob.size/1024/1024)) + " MB)"; link.hidden = false;
        els.recordingHeadline.textContent = "Recording ready to download";
        els.recordingHelp.textContent = "Saved locally in your browser session. Download it before leaving this page.";
        els.recordButton.disabled = false; els.stopButton.disabled = true;
        els.recIndicator.innerHTML = "<i></i> REC OFF"; els.liveDot.classList.remove("active");
        clearInterval(state.timer); els.recordTime.textContent = "00:00";
        note("Recording finished · download it to keep your clip");
        canvasStream.getTracks().forEach(t => t.stop());
      };
      state.recorder.start(1000);
      state.startedAt = Date.now();
      els.recordButton.disabled = true; els.stopButton.disabled = false;
      els.recordingHeadline.textContent = "Recording in progress";
      els.recordingHelp.textContent = "Keep this page open until you stop and download the recording.";
      els.recIndicator.innerHTML = "<i></i> REC ON"; els.liveDot.classList.add("active");
      note("Recording locally · not broadcasting");
      state.timer = setInterval(() => {
        const s = Math.floor((Date.now()-state.startedAt)/1000);
        els.recordTime.textContent = String(Math.floor(s/60)).padStart(2,"0")+":"+String(s%60).padStart(2,"0");
      },250);
    } catch (error) {
      canvasStream.getTracks().forEach(t => t.stop());
      alert("Recording could not start: " + (error.message || error.name));
    }
  }
  function stopRecording() {
    if (state.recorder && state.recorder.state !== "inactive") state.recorder.stop();
  }
  document.addEventListener("keydown", event => {
    if (event.altKey && !event.ctrlKey && !event.metaKey) {
      const tag = event.target?.tagName?.toLowerCase();
      if (["input","textarea","select"].includes(tag) || event.target?.isContentEditable) return;
      const key = event.key.toLowerCase();
      if (key === "1") { event.preventDefault(); setScene("camera"); }
      else if (key === "2") { event.preventDefault(); setScene("screen"); }
      else if (key === "3") { event.preventDefault(); setScene("pip"); }
      else if (key === "s") { event.preventDefault(); takeSnapshot(); }
      else if (key === "r") { event.preventDefault(); if (state.recorder?.state === "recording") stopRecording(); else startRecording(); }
    }
  });
  els.cameraButton.addEventListener("click", enableCamera);
  els.screenButton.addEventListener("click", shareScreen);
  els.micGain.addEventListener("input", () => { els.micGainValue.value = els.micGain.value + "%"; els.micGainValue.textContent = els.micGain.value + "%"; if (state.gain) state.gain.gain.value = Number(els.micGain.value)/100; });
  els.sceneButtons.forEach(b => b.addEventListener("click", () => setScene(b.dataset.scene)));
  [els.showTitle,els.showSubtitle,els.overlayToggle,els.resolutionSelect,els.fpsSelect].forEach(el => el.addEventListener("input", draw));
  els.resolutionSelect.addEventListener("change", draw);
  els.snapshotButton.addEventListener("click", takeSnapshot);
  els.recordButton.addEventListener("click", startRecording);
  els.stopButton.addEventListener("click", stopRecording);
  els.saveProfileButton.addEventListener("click", () => {
    try { localStorage.setItem(state.profileKey, JSON.stringify(profile())); els.profileStatus.textContent = "Profile saved on this device."; }
    catch (error) { els.profileStatus.textContent = "Could not save profile: " + error.message; }
  });
  els.exportProfileButton.addEventListener("click", () => downloadBlob(new Blob([JSON.stringify(profile(),null,2)],{type:"application/json"}),"jt-stream-profile.json"));
  els.importProfileButton.addEventListener("click", () => els.profileFile.click());
  els.profileFile.addEventListener("change", async () => {
    const file = els.profileFile.files?.[0]; if (!file) return;
    try { applyProfile(JSON.parse(await file.text())); els.profileStatus.textContent = "Profile imported."; }
    catch (error) { els.profileStatus.textContent = "Import failed: " + error.message; }
    finally { els.profileFile.value = ""; }
  });
  els.cameraSelect.addEventListener("change", () => { if (state.cameraStream) enableCamera(); });
  els.micSelect.addEventListener("change", () => { if (state.cameraStream) enableCamera(); });
  window.addEventListener("beforeunload", () => {
    if (state.recorder?.state === "recording") state.recorder.stop();
    [state.cameraStream,state.screenStream].filter(Boolean).forEach(s=>s.getTracks().forEach(t=>t.stop()));
    if (state.audioContext && state.audioContext.state !== "closed") state.audioContext.close().catch(() => {});
    if (state.recordingUrl) URL.revokeObjectURL(state.recordingUrl);
  });
  (async () => {
    if (!secureCaptureAvailable()) {
      els.permissionNote.innerHTML = "<strong>Browser access required</strong><span>Open JT Stream on HTTPS or localhost in a current browser to enable camera and screen capture.</span>";
    }
    try {
      const saved = localStorage.getItem(state.profileKey);
      if (saved) { applyProfile(JSON.parse(saved)); els.profileStatus.textContent = "Restored saved profile from this device."; }
    } catch (error) { console.warn("Saved profile could not be loaded", error); }
    els.micGainValue.textContent = els.micGain.value + "%";
    await refreshDevices();
    draw();
  })();
})();