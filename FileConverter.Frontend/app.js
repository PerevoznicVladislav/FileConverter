// Swagger Port
const API_BASE = "http://localhost:7210";

/**
 * Standard message box (top of cards)
 */
function setMsg(text, type = "error") {
  const el = document.getElementById("msg");
  if (!el) return;

  if (!text) {
    el.classList.add("hidden");
    el.innerText = "";
    return;
  }

  el.classList.remove("hidden");
  el.innerText = text;

  // reset classes
  el.className = "mb-4 rounded-lg px-4 py-3 text-sm border";

  if (type === "success") {
    el.classList.add("bg-green-50", "text-green-700", "border-green-200");
  } else {
    el.classList.add("bg-red-50", "text-red-700", "border-red-200");
  }
}

/**
 * Neon / Premium styled message when Free limit is reached
 * (renders inside #result so it looks like part of the conversion flow)
 */
function renderLimitMessage(message) {
  const resultDiv = document.getElementById("result");
  if (!resultDiv) return;

  // prevenim HTML injection
  const safeMsg = String(message)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;");

  resultDiv.innerHTML = `
    <div class="
      relative mt-4 rounded-2xl p-5
      border border-amber-300/30
      bg-amber-500/10 backdrop-blur-xl
      shadow-[0_18px_45px_-22px_rgba(251,191,36,.85)]
    ">
      <div class="pointer-events-none absolute inset-0 rounded-2xl ring-1 ring-amber-300/30"></div>

      <div class="flex gap-4">
        <!-- icon -->
        <div class="mt-0.5 flex-shrink-0 h-10 w-10 rounded-xl
                    bg-amber-400/20 text-amber-300
                    grid place-items-center shadow-inner">
          <svg xmlns="http://www.w3.org/2000/svg"
               class="h-5 w-5"
               viewBox="0 0 24 24" fill="none"
               stroke="currentColor" stroke-width="2">
            <path d="M12 9v4"/>
            <path d="M12 17h.01"/>
            <path d="M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/>
          </svg>
        </div>

        <!-- text -->
        <div class="flex-1">
          <h4 class="font-extrabold text-amber-200 text-sm">
            Monthly limit reached
          </h4>

          <p class="mt-1 text-sm text-amber-100/90">
            ${safeMsg}
          </p>

          <p class="mt-3 text-xs text-amber-100/70">
            You can upgrade your plan by pressing the “Upgrade to Premium for $10” button.
          </p>
        </div>
      </div>
    </div>
  `;
}


// REGISTER
async function registerUser() {
  setMsg("");

  const email = document.getElementById("email").value.trim();
  const password = document.getElementById("password").value;

  const res = await fetch(`${API_BASE}/api/Auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password })
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    setMsg(err.message || "Register failed");
    return;
  }

  setMsg("Registered successfully. Now login.", "success");

  // NOTE: Linia asta îți seta mesaj de eroare chiar după success.
  // Dacă o lași, vei vedea "Login failed" deși registrarea a reușit.
  // setMsg("Login failed: ...", "error");
}

// LOGIN
async function login() {
  setMsg("");

  const email = document.getElementById("email").value.trim();
  const password = document.getElementById("password").value;

  const res = await fetch(`${API_BASE}/api/Auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password })
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    setMsg(err.message || "Login failed");
    return;
  }

  const data = await res.json();
  localStorage.setItem("token", data.token);

  window.location.href = "convert.html";
}

// LOGOUT
function logout() {
  localStorage.removeItem("token");
  window.location.href = "login.html";
}

// CONVERT
async function convertPdf() {
  setMsg("");

  const token = localStorage.getItem("token");
  const input = document.getElementById("fileInput");
  const file = input?.files?.[0];

  if (!file) {
    setMsg("Choose a PDF file first.");
    return;
  }

  const formData = new FormData();
  formData.append("file", file);

  const res = await fetch(`${API_BASE}/api/conversions/convert`, {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${token}`
    },
    body: formData
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    const msg = err.message || "Conversion failed";

    // Daca e limita depasita: afisam card premium + upgrade box
    if (
      msg.toLowerCase().includes("maximum") ||
      msg.toLowerCase().includes("reached") ||
      msg.toLowerCase().includes("limit")
    ) {
      setMsg(""); // ascunde msg-ul rosu
      renderLimitMessage(msg);

      const upgradeBox = document.getElementById("upgradeBox");
      if (upgradeBox) upgradeBox.classList.remove("hidden");
    } else {
      setMsg(msg, "error");
    }
    return;
  }

  const data = await res.json();

  // Success UI: status + highlighted Download Word button
  const resultDiv = document.getElementById("result");
  if (resultDiv) {
    // prevent HTML injection from server message
    const safeMessage = String(data.message || "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;");

    resultDiv.innerHTML = `
      <div class="mt-4 space-y-3">
        <p class="text-sm text-slate-200">
          <b>Status:</b> <span class="text-emerald-300">${data.status}</span>
        </p>

        <p class="text-sm text-slate-300">
          ${safeMessage}
        </p>

        <button
          onclick="downloadFile('${data.conversionId}')"
          class="
            group relative inline-flex items-center gap-2
            rounded-xl px-6 py-3 mt-2
            font-extrabold text-white tracking-wide
            bg-gradient-to-r from-cyan-500 via-indigo-500 to-fuchsia-500
            shadow-[0_18px_45px_-20px_rgba(34,211,238,.9)]
            hover:brightness-110 transition
            active:scale-[0.98]
          ">

          <svg xmlns="http://www.w3.org/2000/svg"
               class="h-5 w-5 transition-transform group-hover:translate-y-0.5"
               viewBox="0 0 24 24" fill="none"
               stroke="currentColor" stroke-width="2">
            <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
            <path d="M7 10l5 5 5-5"/>
            <path d="M12 15V3"/>
          </svg>

          Download Word

          <span class="pointer-events-none absolute inset-y-0 left-0 w-1/2 -skew-x-12
                       bg-white/30 blur-md opacity-60
                       group-hover:animate-shimmer"></span>
        </button>
      </div>
    `;
  }
}

// DOWNLOAD
async function downloadFile(conversionId) {
  setMsg("");

  const token = localStorage.getItem("token");

  const res = await fetch(`${API_BASE}/api/conversions/${conversionId}/download`, {
    method: "GET",
    headers: {
      "Authorization": `Bearer ${token}`
    }
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    setMsg(err.message || "Download failed");
    return;
  }

  const blob = await res.blob();
  const url = window.URL.createObjectURL(blob);

  const a = document.createElement("a");
  a.href = url;
  a.download = `converted_${conversionId}.docx`;
  document.body.appendChild(a);
  a.click();
  a.remove();

  window.URL.revokeObjectURL(url);
}
