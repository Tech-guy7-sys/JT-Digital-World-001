import assert from "node:assert/strict";
import { onRequestPost } from "../functions/api/jeff.js";

async function invoke(body, origin = "https://jt.example", env = {}) {
  return onRequestPost({
    request: new Request("https://jt.example/api/jeff", {
      method: "POST",
      headers: { "content-type": "application/json", origin },
      body: JSON.stringify(body)
    }),
    env
  });
}

let response = await invoke({});
assert.equal(response.status, 400);
assert.equal((await response.json()).error, "message_required");

response = await invoke({ message: "hello" }, "https://evil.example");
assert.equal(response.status, 403);

response = await invoke({ message: "hello", provider: "not-a-provider" });
assert.equal(response.status, 400);

response = await invoke({ message: "hello", provider: "copilot-cli" });
assert.equal(response.status, 501);

response = await invoke({ message: "hello", provider: "openai" });
assert.equal(response.status, 503);
assert.equal((await response.json()).error, "service_disabled");

response = await invoke({ message: "hello", provider: "openai" }, "https://jt.example", { JEFF_API_ENABLED: "true" });
assert.equal(response.status, 503);
assert.equal((await response.json()).error, "provider_not_configured");

console.log("JT AI gateway validation tests passed.");
