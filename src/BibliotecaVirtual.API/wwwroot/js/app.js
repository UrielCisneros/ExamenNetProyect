/* ===================== Estado global ===================== */
const state = {
  user: null,
  carpetaSeleccionada: null,
  arbol: []
};

// El backend puede devolver el perfil como string o como número según la versión de la sesión guardada.
// Se aceptan ambas formas para compatibilidad.
const PERFIL_GESTOR        = ["AdministracionBiblioteca", "GerenteUniversidad", 1, 2];
const PERFIL_ASISTENTE_STR = "AsistenteBiblioteca";
const PERFIL_ASISTENTE_NUM = 3;

const $  = (sel, root = document) => root.querySelector(sel);
const $$ = (sel, root = document) => Array.from(root.querySelectorAll(sel));

/* ===================== Dialogs (confirm) ===================== */
function confirmar({ titulo, msg, labelOk = "Confirmar", tipo = "danger" }) {
  return new Promise(resolve => {
    const icons = {
      danger:  `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>`,
      warning: `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>`,
      info:    `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>`,
    };
    const btnClass = tipo === "danger" ? "btn danger" : tipo === "warning" ? "btn" : "btn primary";
    $("#dlg-body").innerHTML = `
      <div class="dlg-icon dlg-icon--${tipo}">${icons[tipo] ?? icons.info}</div>
      <h3 class="dlg-title">${titulo}</h3>
      <p class="dlg-msg">${msg}</p>
      <div class="dlg-footer">
        <button class="btn ghost" id="dlg-no">Cancelar</button>
        <button class="${btnClass}" id="dlg-si">${labelOk}</button>
      </div>`;
    const dlg = $("#dialog");
    dlg.classList.remove("hidden");
    const done = v => { dlg.classList.add("hidden"); resolve(v); };
    $("#dlg-si").onclick  = () => done(true);
    $("#dlg-no").onclick  = () => done(false);
    dlg.onclick = e => { if (e.target === dlg) done(false); };
  });
}

function pedirMotivo({ titulo, placeholder = "Escribe el motivo..." }) {
  return new Promise(resolve => {
    $("#dlg-body").innerHTML = `
      <div class="dlg-icon dlg-icon--info">
        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
      </div>
      <h3 class="dlg-title">${titulo}</h3>
      <input id="dlg-input" class="dlg-input" type="text" placeholder="${placeholder}" maxlength="300" autofocus />
      <div class="dlg-footer">
        <button class="btn ghost" id="dlg-no">Cancelar</button>
        <button class="btn primary" id="dlg-si">Confirmar</button>
      </div>`;
    const dlg = $("#dialog");
    dlg.classList.remove("hidden");
    setTimeout(() => $("#dlg-input")?.focus(), 60);
    const done = v => { dlg.classList.add("hidden"); resolve(v); };
    $("#dlg-si").onclick = () => {
      const val = $("#dlg-input").value.trim();
      if (!val) { $("#dlg-input").focus(); return; }
      done(val);
    };
    $("#dlg-no").onclick = () => done(null);
    dlg.onclick = e => { if (e.target === dlg) done(null); };
    $("#dlg-input").onkeydown = e => {
      if (e.key === "Enter")  $("#dlg-si").click();
      if (e.key === "Escape") done(null);
    };
  });
}

/* ===================== Toast & modal ===================== */
function toast(msg, tipo = "info") {
  const t = $("#toast");
  t.textContent = msg;
  t.className = "toast " + tipo;
  t.classList.remove("hidden");
  clearTimeout(t._timer);
  t._timer = setTimeout(() => t.classList.add("hidden"), 3500);
}

function abrirModal(titulo, html) {
  $("#modal-titulo").textContent = titulo;
  $("#modal-body").innerHTML = html;
  $("#modal").classList.remove("hidden");
}
function cerrarModal() { $("#modal").classList.add("hidden"); }
document.addEventListener("click", (e) => {
  if (e.target.matches("[data-close-modal]") || e.target.id === "modal") cerrarModal();
});

/* ===================== Login / sesión ===================== */
$("#form-login").addEventListener("submit", async (e) => {
  e.preventDefault();
  const btn = e.target.querySelector("button[type=submit]");
  btn.disabled = true;
  btn.textContent = "Ingresando...";
  $("#login-error").textContent = "";
  try {
    const data = await API.login($("#login-correo").value.trim(), $("#login-password").value);
    API.setSession(data.token, data.usuario);
    location.reload();
  } catch (err) {
    $("#login-error").textContent = err.message;
    btn.disabled = false;
    btn.textContent = "Ingresar";
  }
});

$("#btn-logout").addEventListener("click", () => {
  API.clear();
  location.reload();
});

function puedeGestionar() {
  const u = state.user;
  if (!u) return false;
  if (PERFIL_GESTOR.includes(u.perfil)) return true;
  const esAsistente = u.perfil === PERFIL_ASISTENTE_STR || u.perfil === PERFIL_ASISTENTE_NUM;
  return esAsistente && u.puedeGestionarArchivos === true;
}

function esAdmin() {
  return !!(state.user && PERFIL_GESTOR.includes(state.user.perfil));
}

/* ===================== Boot ===================== */
function boot() {
  state.user = API.getUser();
  if (!state.user || !API.getToken()) {
    $("#vista-login").classList.remove("hidden");
    $("#vista-app").classList.add("hidden");
    return;
  }

  $("#vista-login").classList.add("hidden");
  $("#vista-app").classList.remove("hidden");
  $("#user-nombre").textContent = state.user.nombreCompleto;
  $("#user-perfil").textContent = state.user.perfil + (state.user.puedeGestionarArchivos ? " · gestor" : "");
  // Iniciales para el avatar
  const iniciales = (state.user.nombreCompleto || "?")
    .split(" ").slice(0, 2).map(p => p[0]).join("").toUpperCase();
  const avatarEl = $("#nav-avatar");
  if (avatarEl) avatarEl.textContent = iniciales;

  // Pestañas admin (Usuarios / Logs)
  if (esAdmin()) $$(".gestion-admin").forEach(el => el.classList.remove("hidden"));

  // Mostrar barra de gestión de archivos/carpetas desde el inicio para gestores
  if (puedeGestionar()) {
    $(".gestion-archivos").classList.remove("hidden");
    // Deshabilitar botones hasta que se seleccione una carpeta
    $("#btn-nueva-subcarpeta").disabled = true;
    $("#btn-subir-archivo").disabled = true;
    $("#btn-eliminar-carpeta").style.display = "none";
    $("#gestion-hint-msg").classList.remove("hidden");
    // Mostrar botón de nueva subcarpeta en el sidebar
    $("#btn-nueva-subcarpeta-sidebar").classList.remove("hidden");
  }

  inicializarTabs();
  cargarArbol();
}

function inicializarTabs() {
  $$(".nav-item").forEach(t =>
    t.addEventListener("click", () => activarTab(t.dataset.tab))
  );
}

function activarTab(name) {
  $$(".nav-item").forEach(t => t.classList.toggle("active", t.dataset.tab === name));
  $$(".tab-content").forEach(c => c.classList.toggle("active", c.id === "tab-" + name));
  $$(".tab-content").forEach(c => c.classList.toggle("hidden", c.id !== "tab-" + name));
  if (name === "usuarios") cargarUsuarios();
  if (name === "logs") cargarLogs();
}

/* ===================== Árbol de carpetas ===================== */
$("#btn-recargar-arbol").addEventListener("click", cargarArbol);

async function cargarArbol() {
  const btn = $("#btn-recargar-arbol");
  btn.disabled = true;
  try {
    state.arbol = await API.arbolCarpetas();
    renderArbol();
  } catch (err) {
    toast(err.message, "error");
  } finally {
    btn.disabled = false;
  }
}

function renderArbol() {
  const cont = $("#arbol-carpetas");
  cont.innerHTML = "";
  if (!state.arbol.length) {
    cont.innerHTML = '<small class="muted" style="padding:8px">Sin carpetas disponibles. Recarga la página para reintentar.</small>';
    // Ocultar botón Nueva cuando no hay árbol (nada puede ser carpeta padre)
    $("#btn-nueva-subcarpeta-sidebar").classList.add("hidden");
    return;
  }
  // Restaurar visibilidad del botón si el usuario puede gestionar
  if (puedeGestionar()) $("#btn-nueva-subcarpeta-sidebar").classList.remove("hidden");

  const nodoHtml = (carpeta, esRaiz = false) => {
    const div = document.createElement("div");

    const nodo = document.createElement("div");
    nodo.className = "nodo" + (esRaiz ? " raiz" : "");
    nodo.dataset.id = carpeta.id;

    const tieneHijos = carpeta.subcarpetas?.length > 0;
    const icono = esRaiz && carpeta.banco ? "🏦" : "📁";
    nodo.innerHTML = `
      <span class="toggler">${tieneHijos ? "▸" : "·"}</span>
      <span class="nodo-label">${icono} ${escapeHtml(carpeta.nombre)}</span>
      <span class="nodo-count">${carpeta.cantidadArchivos || 0}</span>
    `;
    nodo.addEventListener("click", (e) => {
      e.stopPropagation();
      seleccionarCarpeta(carpeta.id);
      const hijos = div.querySelector(".hijos");
      if (hijos) {
        hijos.classList.toggle("hidden");
        const tog = nodo.querySelector(".toggler");
        if (tog) tog.textContent = hijos.classList.contains("hidden") ? "▸" : "▾";
      }
      $$(".arbol .nodo").forEach(n => n.classList.remove("activo"));
      nodo.classList.add("activo");
    });
    div.appendChild(nodo);

    if (tieneHijos) {
      const hijos = document.createElement("div");
      hijos.className = "hijos hidden";
      carpeta.subcarpetas.forEach(s => hijos.appendChild(nodoHtml(s)));
      div.appendChild(hijos);
    }
    return div;
  };

  state.arbol.forEach(raiz => cont.appendChild(nodoHtml(raiz, true)));
}

/* ===================== Selección de carpeta ===================== */
async function seleccionarCarpeta(id) {
  try {
    const c = await API.obtenerCarpeta(id);
    state.carpetaSeleccionada = c;

    // Breadcrumb
    const partes = [];
    if (c.banco) partes.push(`🏦 ${c.banco}`);
    partes.push(c.nombre);
    if (c.esRaiz) partes.push("(raíz)");
    $("#breadcrumb").textContent = partes.join(" › ");

    // Barra de acciones
    const puedeGest = puedeGestionar();
    if (puedeGest) {
      $(".gestion-archivos").classList.remove("hidden");
      // Habilitar botones ahora que hay una carpeta seleccionada
      $("#btn-nueva-subcarpeta").disabled = false;
      $("#btn-subir-archivo").disabled = false;
      // "Eliminar carpeta" solo en carpetas no-raíz
      $("#btn-eliminar-carpeta").style.display = !c.esRaiz ? "" : "none";
      // Ocultar el hint inicial
      const hint = $("#gestion-hint-msg");
      if (hint) hint.classList.add("hidden");
    }

    // ── Subcarpetas ──
    const subs = await API.carpetasHijas(id);
    renderSubcarpetas(subs, puedeGest);

    // ── Archivos ──
    const archivos = await API.listarArchivos(id);
    renderArchivos(archivos, puedeGest);

  } catch (err) {
    toast(err.message, "error");
  }
}

function renderSubcarpetas(subs, puedeGest) {
  const grid = $("#lista-subcarpetas");
  grid.innerHTML = "";
  if (!subs.length) {
    grid.innerHTML = '<small class="muted">Esta carpeta no tiene subcarpetas.</small>';
    return;
  }
  subs.forEach(s => {
    const card = document.createElement("div");
    card.className = "card-carpeta";
    card.innerHTML = `
      <div class="nombre">📁 ${escapeHtml(s.nombre)}</div>
      ${s.descripcion ? `<small class="muted">${escapeHtml(s.descripcion)}</small>` : ""}
    `;
    card.addEventListener("click", () => {
      $$(".arbol .nodo").forEach(n => {
        if (n.dataset.id == s.id) {
          n.classList.add("activo");
          // expandir el nodo padre si estaba colapsado
          const hijos = n.closest(".hijos");
          if (hijos) hijos.classList.remove("hidden");
        } else {
          n.classList.remove("activo");
        }
      });
      seleccionarCarpeta(s.id);
    });
    grid.appendChild(card);
  });
}

const FILE_ICONS = {
  ".pdf":  { label: "PDF",  color: "#ef4444", bg: "#fee2e2" },
  ".docx": { label: "DOC",  color: "#3b82f6", bg: "#dbeafe" },
  ".pptx": { label: "PPT",  color: "#f97316", bg: "#ffedd5" },
  ".xlsx": { label: "XLS",  color: "#16a34a", bg: "#dcfce7" },
  ".mov":  { label: "MOV",  color: "#8b5cf6", bg: "#ede9fe" },
  ".avi":  { label: "AVI",  color: "#6366f1", bg: "#e0e7ff" },
  ".mp3":  { label: "MP3",  color: "#ec4899", bg: "#fce7f3" },
};

function iconoArchivo(ext) {
  const k = (ext || "").toLowerCase();
  const i = FILE_ICONS[k] ?? { label: k.replace(".", "").toUpperCase().slice(0, 3) || "?", color: "#64748b", bg: "#f1f5f9" };
  return `<span class="file-icon" style="background:${i.bg};color:${i.color}">${i.label}</span>`;
}

function renderArchivos(archivos, puedeGest) {
  const tbody = $("#tabla-archivos");
  tbody.innerHTML = "";
  if (!archivos.length) {
    tbody.innerHTML = '<tr><td colspan="6" class="text-center"><small class="muted">Esta carpeta no tiene archivos.</small></td></tr>';
    return;
  }
  archivos.forEach(a => {
    const tr = document.createElement("tr");
    tr.innerHTML = `
      <td>
        <div class="file-cell">
          ${iconoArchivo(a.extension)}
          <div class="file-cell-info">
            <span class="file-nombre" title="${escapeAttr(a.nombre + a.extension)}">${escapeHtml(a.nombre)}</span>
            <span class="file-ext-label">${escapeHtml(a.extension)}</span>
          </div>
        </div>
      </td>
      <td><span class="badge tipo-${(a.tipo || '').toLowerCase()}">${a.tipo || '—'}</span></td>
      <td>${formatBytes(a.tamanoBytes)}</td>
      <td>${escapeHtml(a.correoCreador)}</td>
      <td>${fmtFecha(a.fechaCreacion)}</td>
      <td class="acciones">
        <button class="btn small" title="Descargar" data-descargar="${a.id}" data-nombre="${escapeAttr(a.nombre + a.extension)}">⬇ Descargar</button>
        ${puedeGest ? `<button class="btn small secondary" title="Editar" data-editar="${a.id}" data-nombre="${escapeAttr(a.nombre)}" data-desc="${escapeAttr(a.descripcion || '')}">✎ Editar</button>` : ""}
        ${puedeGest ? `<button class="btn small danger" title="Eliminar" data-eliminar="${a.id}">🗑 Eliminar</button>` : ""}
      </td>
    `;
    tbody.appendChild(tr);
  });
}

/* ===================== Acciones sobre archivos ===================== */
document.addEventListener("click", async (e) => {
  // Descargar
  const dl = e.target.closest("[data-descargar]");
  if (dl) {
    dl.disabled = true;
    dl.textContent = "⏳";
    try {
      await API.descargarArchivoBlob(dl.dataset.descargar, dl.dataset.nombre);
      toast("Descarga iniciada", "success");
    } catch (err) {
      toast(err.message, "error");
    } finally {
      dl.disabled = false;
      dl.textContent = "⬇ Descargar";
    }
    return;
  }

  // Editar
  const ed = e.target.closest("[data-editar]");
  if (ed) {
    modalEditarArchivo(ed.dataset.editar, ed.dataset.nombre, ed.dataset.desc);
    return;
  }

  // Eliminar
  const el = e.target.closest("[data-eliminar]");
  if (el) {
    const ok = await confirmar({
      titulo: "¿Eliminar archivo?",
      msg: "El registro se eliminará de forma lógica. El archivo físico se borrará automáticamente en 2 minutos.",
      labelOk: "Eliminar",
      tipo: "danger"
    });
    if (!ok) return;
    try {
      await API.eliminarArchivo(el.dataset.eliminar);
      toast("Archivo eliminado. El archivo físico se borrará en 2 minutos.", "success");
      seleccionarCarpeta(state.carpetaSeleccionada.id);
      cargarArbol();
    } catch (err) {
      toast(err.message, "error");
    }
  }
});

/* ===================== Nueva subcarpeta ===================== */
function abrirModalNuevaCarpeta(padre) {
  abrirModal(`Nueva subcarpeta en "${padre.nombre}"`, `
    <form id="form-nueva-carpeta">
      <label>Nombre <span class="req">*</span></label>
      <input id="c-nombre" maxlength="120" placeholder="Nombre de la subcarpeta" required autofocus />
      <label>Descripción <small class="muted">(opcional, máx. 500 caracteres)</small></label>
      <textarea id="c-desc" maxlength="500" rows="3" placeholder="Descripción de la subcarpeta..."></textarea>
      <p class="muted" style="font-size:0.82em;">Se creará dentro de: <strong>${escapeHtml(padre.nombre)}</strong></p>
      <div class="acciones-modal">
        <button type="button" class="btn ghost" data-close-modal>Cancelar</button>
        <button type="submit" id="btn-crear-carpeta" class="btn primary">＋ Crear subcarpeta</button>
      </div>
    </form>`);

  $("#form-nueva-carpeta").addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const btn = $("#btn-crear-carpeta");
    btn.disabled = true;
    btn.textContent = "Creando...";
    try {
      await API.crearCarpeta({
        nombre: $("#c-nombre").value.trim(),
        descripcion: $("#c-desc").value.trim() || null,
        carpetaPadreId: padre.id
      });
      cerrarModal();
      toast(`Subcarpeta "${$("#c-nombre")?.value || ''}" creada correctamente.`, "success");
      await cargarArbol();
      await seleccionarCarpeta(padre.id);
    } catch (err) {
      toast(err.message, "error");
      btn.disabled = false;
      btn.textContent = "＋ Crear subcarpeta";
    }
  });
}

$("#btn-nueva-subcarpeta").addEventListener("click", () => {
  if (!state.carpetaSeleccionada) return toast("Selecciona primero una carpeta padre.", "error");
  abrirModalNuevaCarpeta(state.carpetaSeleccionada);
});

$("#btn-nueva-subcarpeta-sidebar").addEventListener("click", () => {
  if (!state.carpetaSeleccionada) return toast("Haz clic en una carpeta del árbol para seleccionarla y luego presiona Nueva.", "info");
  abrirModalNuevaCarpeta(state.carpetaSeleccionada);
});

/* ===================== Eliminar carpeta ===================== */
$("#btn-eliminar-carpeta").addEventListener("click", async () => {
  if (!state.carpetaSeleccionada) return;
  const c = state.carpetaSeleccionada;
  const okCarpeta = await confirmar({
    titulo: `¿Eliminar carpeta?`,
    msg: `La carpeta <strong>${escapeHtml(c.nombre)}</strong> se eliminará. Solo es posible si está vacía (sin archivos ni subcarpetas activas).`,
    labelOk: "Eliminar",
    tipo: "danger"
  });
  if (!okCarpeta) return;

  const btn = $("#btn-eliminar-carpeta");
  btn.disabled = true;
  btn.textContent = "Eliminando...";
  try {
    await API.eliminarCarpeta(c.id);
    toast(`Carpeta "${c.nombre}" eliminada.`, "success");
    state.carpetaSeleccionada = null;
    $("#breadcrumb").textContent = "Selecciona una carpeta";
    // Volver a estado inicial: barra visible pero botones deshabilitados
    $("#btn-nueva-subcarpeta").disabled = true;
    $("#btn-subir-archivo").disabled = true;
    $("#btn-eliminar-carpeta").style.display = "none";
    const hint = $("#gestion-hint-msg");
    if (hint) hint.classList.remove("hidden");
    $("#lista-subcarpetas").innerHTML = "";
    $("#tabla-archivos").innerHTML = "";
    await cargarArbol();
  } catch (err) {
    toast(err.message, "error");
    btn.disabled = false;
    btn.textContent = "🗑 Eliminar carpeta";
  }
});

/* ===================== Subir archivo ===================== */
$("#btn-subir-archivo").addEventListener("click", () => {
  if (!state.carpetaSeleccionada) return toast("Selecciona primero la carpeta destino.", "error");
  const destino = state.carpetaSeleccionada;

  abrirModal(`Subir archivo en "${destino.nombre}"`, `
    <form id="form-subir-archivo">
      <label>Nombre del archivo <span class="req">*</span> <small class="muted">(máx. 50 caracteres)</small></label>
      <input id="a-nombre" maxlength="50" placeholder="Nombre descriptivo del archivo" required autofocus />

      <label>Descripción <span class="req">*</span> <small class="muted">(máx. 500 caracteres)</small></label>
      <textarea id="a-desc" maxlength="500" rows="3" placeholder="Describe el contenido del archivo..." required></textarea>

      <label>Archivo <span class="req">*</span></label>
      <small class="muted">Formatos permitidos: .pdf · .docx · .pptx · .xlsx · .mov · .avi · .mp3</small>
      <input id="a-file" type="file" accept=".pdf,.docx,.pptx,.xlsx,.mov,.avi,.mp3" required style="margin-top:4px;" />

      <div id="a-progreso" class="hidden" style="margin-top:8px;">
        <progress style="width:100%"></progress>
        <small class="muted" id="a-progreso-msg">Subiendo archivo...</small>
      </div>

      <div class="acciones-modal">
        <button type="button" class="btn ghost" data-close-modal>Cancelar</button>
        <button type="submit" id="btn-subir-confirmar" class="btn primary">⬆ Subir archivo</button>
      </div>
    </form>`);

  // Autocompletar nombre desde nombre del archivo
  document.getElementById("a-file").addEventListener("change", (ev) => {
    const f = ev.target.files[0];
    if (!f) return;
    const input = document.getElementById("a-nombre");
    if (!input.value) {
      // Quitar extensión del nombre de archivo como sugerencia
      const nombreSugerido = f.name.replace(/\.[^/.]+$/, "").substring(0, 50);
      input.value = nombreSugerido;
    }
  });

  document.getElementById("form-subir-archivo").addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const file = document.getElementById("a-file").files[0];
    if (!file) return toast("Selecciona un archivo.", "error");

    const btnSubir = document.getElementById("btn-subir-confirmar");
    const progreso = document.getElementById("a-progreso");
    const progresoMsg = document.getElementById("a-progreso-msg");

    btnSubir.disabled = true;
    btnSubir.textContent = "Subiendo...";
    progreso.classList.remove("hidden");
    progresoMsg.textContent = `Subiendo "${file.name}" (${formatBytes(file.size)})...`;

    try {
      await API.subirArchivo(
        destino.id,
        document.getElementById("a-nombre").value.trim(),
        document.getElementById("a-desc").value.trim(),
        file
      );
      cerrarModal();
      toast("Archivo subido correctamente.", "success");
      await seleccionarCarpeta(destino.id);
      await cargarArbol();
    } catch (err) {
      toast(err.message, "error");
      btnSubir.disabled = false;
      btnSubir.textContent = "⬆ Subir archivo";
      progreso.classList.add("hidden");
    }
  });
});

/* ===================== Editar archivo ===================== */
function modalEditarArchivo(id, nombre, descripcion) {
  abrirModal("Editar archivo", `
    <form id="form-editar-archivo">
      <label>Nombre <span class="req">*</span> <small class="muted">(máx. 50 caracteres)</small></label>
      <input id="e-nombre" maxlength="50" value="${escapeAttr(nombre)}" required autofocus />
      <label>Descripción <span class="req">*</span> <small class="muted">(máx. 500 caracteres)</small></label>
      <textarea id="e-desc" maxlength="500" rows="3" required>${escapeHtml(descripcion)}</textarea>
      <div class="acciones-modal">
        <button type="button" class="btn ghost" data-close-modal>Cancelar</button>
        <button type="submit" id="btn-editar-confirmar" class="btn primary">Guardar cambios</button>
      </div>
    </form>`);

  document.getElementById("form-editar-archivo").addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const btn = document.getElementById("btn-editar-confirmar");
    btn.disabled = true;
    btn.textContent = "Guardando...";
    try {
      await API.editarArchivo(id, {
        nombre: document.getElementById("e-nombre").value.trim(),
        descripcion: document.getElementById("e-desc").value.trim()
      });
      cerrarModal();
      toast("Archivo actualizado correctamente.", "success");
      seleccionarCarpeta(state.carpetaSeleccionada.id);
    } catch (err) {
      toast(err.message, "error");
      btn.disabled = false;
      btn.textContent = "Guardar cambios";
    }
  });
}

/* ===================== Usuarios ===================== */
$("#chk-incluir-inactivos").addEventListener("change", cargarUsuarios);

$("#btn-nuevo-usuario").addEventListener("click", () => {
  abrirModal("Nuevo usuario", `
    <form id="form-nuevo-usuario">
      <label>Correo <span class="req">*</span> <small class="muted">(identificador único, no modificable)</small></label>
      <input id="u-correo" type="email" required autofocus />
      <label>Nombre completo <span class="req">*</span></label>
      <input id="u-nombre" required maxlength="150" />
      <label>Nombre de usuario <span class="req">*</span></label>
      <input id="u-usuario" required maxlength="80" />
      <label>Contraseña <span class="req">*</span></label>
      <input id="u-pass" type="password" required minlength="6" />
      <label>Perfil</label>
      <select id="u-perfil">
        <option value="1">AdministracionBiblioteca</option>
        <option value="2">GerenteUniversidad</option>
        <option value="3" selected>AsistenteBiblioteca</option>
        <option value="4">Empleado</option>
      </select>
      <div class="acciones-modal">
        <button type="button" class="btn ghost" data-close-modal>Cancelar</button>
        <button type="submit" id="btn-crear-usuario" class="btn primary">Crear usuario</button>
      </div>
    </form>`);

  document.getElementById("form-nuevo-usuario").addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const btn = document.getElementById("btn-crear-usuario");
    btn.disabled = true;
    btn.textContent = "Creando...";
    try {
      await API.crearUsuario({
        correo: document.getElementById("u-correo").value.trim(),
        nombreCompleto: document.getElementById("u-nombre").value.trim(),
        nombreUsuario: document.getElementById("u-usuario").value.trim(),
        password: document.getElementById("u-pass").value,
        perfil: parseInt(document.getElementById("u-perfil").value, 10)
      });
      cerrarModal();
      toast("Usuario creado correctamente.", "success");
      cargarUsuarios();
    } catch (err) {
      toast(err.message, "error");
      btn.disabled = false;
      btn.textContent = "Crear usuario";
    }
  });
});

async function cargarUsuarios() {
  try {
    const incl = $("#chk-incluir-inactivos").checked;
    const lista = await API.listarUsuarios(incl);
    const tbody = $("#tabla-usuarios");
    tbody.innerHTML = "";
    if (!lista.length) {
      tbody.innerHTML = '<tr><td colspan="8"><small class="muted">Sin usuarios.</small></td></tr>';
      return;
    }
    lista.forEach(u => {
      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td>${escapeHtml(u.correo)}</td>
        <td>${escapeHtml(u.nombreCompleto)}</td>
        <td>${escapeHtml(u.nombreUsuario)}</td>
        <td>${u.perfil}</td>
        <td><span class="badge ${u.activo ? "activo" : "inactivo"}">${u.activo ? "Activo" : "Inactivo"}</span></td>
        <td>${u.puedeGestionarArchivos ? "✅ Gestor" : "—"}</td>
        <td>${fmtFecha(u.fechaCreacion, true)}</td>
        <td class="acciones">
          ${u.activo
            ? `<button class="btn small danger" data-inactivar="${escapeAttr(u.correo)}">Inactivar</button>`
            : `<button class="btn small" data-reactivar="${escapeAttr(u.correo)}">Reactivar</button>`}
          ${u.perfil === "AsistenteBiblioteca"
            ? `<button class="btn small secondary" data-permiso="${escapeAttr(u.correo)}" data-actual="${u.puedeGestionarArchivos}">
                ${u.puedeGestionarArchivos ? "Revocar permiso" : "Otorgar permiso"}
               </button>` : ""}
        </td>`;
      tbody.appendChild(tr);
    });
  } catch (err) { toast(err.message, "error"); }
}

document.addEventListener("click", async (e) => {
  const ina = e.target.closest("[data-inactivar]");
  if (ina) {
    const motivo = await pedirMotivo({ titulo: "Inactivar usuario", placeholder: "Motivo de inactivación..." });
    if (!motivo) return;
    try { await API.inactivarUsuario(ina.dataset.inactivar, motivo); toast("Usuario inactivado.", "success"); cargarUsuarios(); }
    catch (err) { toast(err.message, "error"); }
    return;
  }
  const rea = e.target.closest("[data-reactivar]");
  if (rea) {
    const motivo = await pedirMotivo({ titulo: "Reactivar usuario", placeholder: "Motivo de reactivación..." });
    if (!motivo) return;
    try { await API.reactivarUsuario(rea.dataset.reactivar, motivo); toast("Usuario reactivado.", "success"); cargarUsuarios(); }
    catch (err) { toast(err.message, "error"); }
    return;
  }
  const per = e.target.closest("[data-permiso]");
  if (per) {
    const actual = per.dataset.actual === "true";
    try { await API.actualizarPermisos(per.dataset.permiso, !actual); toast("Permisos actualizados.", "success"); cargarUsuarios(); }
    catch (err) { toast(err.message, "error"); }
  }
});

/* ===================== Logs ===================== */
$("#btn-filtrar-logs").addEventListener("click", cargarLogs);

async function cargarLogs() {
  const btn = $("#btn-filtrar-logs");
  btn.disabled = true;
  btn.textContent = "Buscando...";
  try {
    const data = await API.buscarLogs({
      correo: $("#log-correo").value.trim(),
      entidad: $("#log-entidad").value,
      accion: $("#log-accion").value,
      pagina: 1,
      tamano: 100
    });
    const tbody = $("#tabla-logs");
    tbody.innerHTML = "";
    if (!data.items.length) {
      tbody.innerHTML = '<tr><td colspan="5"><small class="muted">Sin resultados para ese filtro.</small></td></tr>';
      return;
    }
    data.items.forEach(l => {
      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td>${fmtFecha(l.fecha)}</td>
        <td>${escapeHtml(l.correoUsuario)}</td>
        <td><span class="badge accion-${(l.accion || '').toLowerCase()}">${l.accion}</span></td>
        <td>${l.entidad}${l.idEntidad ? " #" + l.idEntidad : ""}</td>
        <td>${escapeHtml(l.detalle)}</td>`;
      tbody.appendChild(tr);
    });
  } catch (err) {
    toast(err.message, "error");
  } finally {
    btn.disabled = false;
    btn.textContent = "Buscar";
  }
}

/* ===================== Helpers ===================== */
function fmtFecha(s, solo_fecha = false) {
  if (!s) return "—";
  // El API devuelve UTC sin 'Z'; forzamos interpretación UTC para que
  // toLocaleString convierta correctamente a la hora local del navegador.
  const iso = s.endsWith("Z") || s.includes("+") ? s : s + "Z";
  const d = new Date(iso);
  return solo_fecha ? d.toLocaleDateString("es-MX") : d.toLocaleString("es-MX");
}

function formatBytes(n) {
  if (!n || n < 1024) return (n || 0) + " B";
  const u = ["KB", "MB", "GB"]; let i = -1;
  do { n /= 1024; i++; } while (n >= 1024 && i < u.length - 1);
  return n.toFixed(1) + " " + u[i];
}
function escapeHtml(s) {
  return (s || "").toString()
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;");
}
function escapeAttr(s) {
  return escapeHtml(s).replaceAll('"', "&quot;");
}

/* ===================== Init ===================== */
boot();
