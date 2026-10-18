# JT Uninstall Feedback

Each app's NSIS uninstaller now offers a single-choice reason page and an optional details field. Choosing a reason is optional; users can select “I prefer not to say — continue uninstalling” or continue without selecting a reason. The uninstaller writes a local feedback text file to `%APPDATA%\JT Digital World\Command-Agent\uninstall-feedback`.

The JT Command Agent imports queued files when `npm run command:status` or `npm run command:report` is run, appends the feedback to `command-agent/workspace/state.json`, and archives a JSON record under `command-agent/workspace/uninstall-feedback`. This is local-only: it does not transmit the feedback over the internet. If you want centralized reporting across different computers, that requires an explicitly configured, disclosed server endpoint and consent/privacy review.

The installer/uninstaller must be rebuilt on Windows to validate NSIS compilation and the visual appearance of the custom page. NSIS native radio buttons enforce one selected option; the exact selected indicator color is controlled by the Windows theme.
