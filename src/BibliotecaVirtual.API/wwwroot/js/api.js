/* Cliente HTTP centralizado para la API de Biblioteca Virtual. */
const API = (() => {
  const TOKEN_KEY = "bv_token";
  const USER_KEY = "bv_user";

  const getToken = () => localStorage.getItem(TOKEN_KEY);
  const getUser  = () => JSON.parse(localStorage.getItem(USER_KEY) || "null");
  const setSession = (token, usuario) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, JSON.stringify(usuario));
  };
  const clear = () => { localStorage.removeItem(TOKEN_KEY); localStorage.removeItem(USER_KEY); };

  async function request(path, opts = {}) {
    const headers = opts.headers ? { ...opts.headers } : {};
    const t = getToken();
    if (t) headers["Authorization"] = "Bearer " + t;
    if (opts.body && !(opts.body instanceof FormData) && !headers["Content-Type"]) {
      headers["Content-Type"] = "application/json";
    }
    const r = await fetch(path, { ...opts, headers });

    if (r.status === 401) {
      clear();
      location.reload();
      throw new Error("Sesión expirada");
    }

    let data = null;
    const text = await r.text();
    if (text) {
      try { data = JSON.parse(text); }
      catch { data = text; }
    }

    if (!r.ok) {
      const msg = (data && (data.error || data.message)) || `Error ${r.status}`;
      const err = new Error(msg);
      err.status = r.status;
      err.payload = data;
      throw err;
    }
    return data;
  }

  return {
    getToken, getUser, clear, setSession,

    // Auth
    login: (correo, password) =>
      request("/api/auth/login", { method: "POST", body: JSON.stringify({ correo, password }) }),

    // Usuarios
    listarUsuarios: (incluirInactivos = false) =>
      request(`/api/usuarios?incluirInactivos=${incluirInactivos}`),
    crearUsuario: (dto) =>
      request("/api/usuarios", { method: "POST", body: JSON.stringify(dto) }),
    inactivarUsuario: (correo, motivo) =>
      request(`/api/usuarios/${encodeURIComponent(correo)}/inactivar`,
        { method: "PATCH", body: JSON.stringify({ motivo }) }),
    reactivarUsuario: (correo, motivo) =>
      request(`/api/usuarios/${encodeURIComponent(correo)}/reactivar`,
        { method: "PATCH", body: JSON.stringify({ motivo }) }),
    actualizarPermisos: (correo, puedeGestionar) =>
      request(`/api/usuarios/${encodeURIComponent(correo)}/permisos`,
        { method: "PATCH", body: JSON.stringify({ puedeGestionarArchivos: puedeGestionar }) }),

    // Carpetas
    raicesCarpetas: () => request("/api/carpetas/raices"),
    arbolCarpetas: () => request("/api/carpetas/arbol"),
    carpetasHijas: (padreId) => request(`/api/carpetas/${padreId}/hijas`),
    obtenerCarpeta: (id) => request(`/api/carpetas/${id}`),
    crearCarpeta: (dto) => request("/api/carpetas", { method: "POST", body: JSON.stringify(dto) }),
    renombrarCarpeta: (id, dto) => request(`/api/carpetas/${id}`, { method: "PATCH", body: JSON.stringify(dto) }),
    eliminarCarpeta: (id) => request(`/api/carpetas/${id}`, { method: "DELETE" }),

    // Archivos
    listarArchivos: (carpetaId) => request(`/api/archivos/carpeta/${carpetaId}`),
    subirArchivo: (carpetaId, nombre, descripcion, file) => {
      const fd = new FormData();
      fd.append("carpetaId", carpetaId);
      fd.append("nombre", nombre);
      fd.append("descripcion", descripcion);
      fd.append("archivo", file);
      return request("/api/archivos/subir", { method: "POST", body: fd });
    },
    editarArchivo: (id, dto) =>
      request(`/api/archivos/${id}`, { method: "PATCH", body: JSON.stringify(dto) }),
    eliminarArchivo: (id) =>
      request(`/api/archivos/${id}`, { method: "DELETE" }),
    urlDescarga: (id) => `/api/archivos/${id}/descargar`,

    // Logs
    buscarLogs: (filtros) => {
      const qs = new URLSearchParams();
      Object.entries(filtros || {}).forEach(([k, v]) => {
        if (v !== undefined && v !== null && v !== "") qs.append(k, v);
      });
      return request("/api/logs?" + qs.toString());
    },

    async descargarArchivoBlob(id, nombreSugerido) {
      const r = await fetch(`/api/archivos/${id}/descargar`, {
        headers: { Authorization: "Bearer " + getToken() }
      });
      if (!r.ok) throw new Error("No se pudo descargar el archivo.");
      const blob = await r.blob();
      const a = document.createElement("a");
      a.href = URL.createObjectURL(blob);
      a.download = nombreSugerido || "archivo";
      document.body.appendChild(a);
      a.click();
      a.remove();
      URL.revokeObjectURL(a.href);
    }
  };
})();
