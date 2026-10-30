-- Ejecutar con una cuenta autorizada a crear bases y tablas.
-- No incluye usuarios, contraseñas ni instrucciones para borrar datos.
CREATE DATABASE IF NOT EXISTS panelnica CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE panelnica;

CREATE TABLE IF NOT EXISTS ciudades (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    latitud DOUBLE NOT NULL,
    longitud DOUBLE NOT NULL
);

CREATE TABLE IF NOT EXISTS tipo_cambio (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    fecha_utc DATETIME(6) NOT NULL,
    fuente VARCHAR(50) NOT NULL,
    tasa DECIMAL(18,6) NOT NULL,
    fecha_fuente VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS cache_respuestas (
    clave CHAR(64) PRIMARY KEY,
    servicio VARCHAR(100) NOT NULL,
    respuesta LONGTEXT NOT NULL,
    fecha_utc DATETIME(6) NOT NULL
);

-- La aplicación agrega cinco ciudades al pulsar Crear tablas y ciudades iniciales
-- únicamente cuando la tabla ciudades está vacía.
