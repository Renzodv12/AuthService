-- =====================================================
-- Script para crear tabla EmailVerificationCodes
-- Ejecutar este script completo en PostgreSQL
-- =====================================================

-- 1. Habilitar extensión UUID si no está habilitada
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 2. Crear la tabla EmailVerificationCodes
CREATE TABLE IF NOT EXISTS "EmailVerificationCodes" (
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "UserId" uuid NOT NULL,
    "Code" character varying(10) NOT NULL,
    "Email" character varying(100) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "IsUsed" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT now(),
    "UsedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_EmailVerificationCodes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EmailVerificationCodes_Users_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- 3. Crear índices para mejorar rendimiento
CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_UserId" 
    ON "EmailVerificationCodes" ("UserId");

CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_Code" 
    ON "EmailVerificationCodes" ("Code");

CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_UserId_Code_IsUsed" 
    ON "EmailVerificationCodes" ("UserId", "Code", "IsUsed");

-- 4. Agregar comentarios descriptivos
COMMENT ON TABLE "EmailVerificationCodes" IS 'Códigos de verificación de dos factores enviados por email';
COMMENT ON COLUMN "EmailVerificationCodes"."IsUsed" IS 'Indica si el código ya fue utilizado';
COMMENT ON COLUMN "EmailVerificationCodes"."ExpiresAt" IS 'Fecha y hora de expiración del código';

-- Verificar que la tabla se creó correctamente
SELECT 
    table_name,
    column_name,
    data_type,
    character_maximum_length
FROM 
    information_schema.columns 
WHERE 
    table_name = 'EmailVerificationCodes'
ORDER BY 
    ordinal_position;

-- Mostrar mensaje de confirmación
DO $$ 
BEGIN 
    RAISE NOTICE 'Tabla EmailVerificationCodes creada exitosamente';
END $$;

