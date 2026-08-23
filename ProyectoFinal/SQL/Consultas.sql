-- ============================================
-- BASE DE DATOS
-- ============================================

CREATE DATABASE amigurumis_luna;

USE amigurumis_luna;


-- ============================================
-- TABLA USUARIO
-- Para el inicio de sesión del administrador
-- ============================================

CREATE TABLE usuario (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    usuario VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
);


-- ============================================
-- TABLA MODELO
-- Información de los modelos de amigurumis
-- ============================================

CREATE TABLE modelo (
    id_modelo INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT,
    imagen VARCHAR(255),
    dificultad VARCHAR(20) NOT NULL,
    tiempo_produccion DECIMAL(10,2) NOT NULL
);


-- ============================================
-- TABLA MATERIAL
-- Materiales utilizados para elaborar
-- los amigurumis
-- ============================================

CREATE TABLE material (
    id_material INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    tipo_material VARCHAR(50) NOT NULL,
    color VARCHAR(50) NOT NULL,
    unidad_medida VARCHAR(20) NOT NULL,
    stock_minimo DECIMAL(10,2) NOT NULL DEFAULT 0,
    cantidad_disponible DECIMAL(10,2) NOT NULL DEFAULT 0,
    precio_unitario DECIMAL(10,2) NOT NULL,
    estado ENUM('stock disponible', 'stock agotado', 'stock bajo') NOT NULL DEFAULT 'stock disponible'
);
-- ============================================
-- TABLA MODELO_MATERIAL
-- Relaciona modelos con sus materiales
-- ============================================

CREATE TABLE modelo_material (
    id_modelo INT NOT NULL,
    id_material INT NOT NULL,
    cantidad_utilizada DECIMAL(10,2) NOT NULL,

    PRIMARY KEY (id_modelo, id_material),

    FOREIGN KEY (id_modelo)
        REFERENCES modelo(id_modelo)
        ON DELETE CASCADE,

    FOREIGN KEY (id_material)
        REFERENCES material(id_material)
        ON DELETE CASCADE
);