-- =====================================================
-- Script completo para crear todas las tablas necesarias
-- Ejecuta este script en PostgreSQL
-- =====================================================

-- 1. Habilitar extensión UUID si no está habilitada
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 2. Crear tabla PasswordResetToken (sin la "s")
CREATE TABLE IF NOT EXISTS "PasswordResetToken" (
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "UserId" uuid NOT NULL,
    "Token" text NOT NULL,
    "Email" text NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "IsUsed" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UsedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_PasswordResetToken" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PasswordResetToken_Users_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- 3. Crear tabla EmailVerificationToken
CREATE TABLE IF NOT EXISTS "EmailVerificationToken" (
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "UserId" uuid NOT NULL,
    "Token" text NOT NULL,
    "Email" text NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "IsUsed" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UsedAt" timestamp with time zone NULL,
    CONSTRAINT "PK_EmailVerificationToken" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EmailVerificationToken_Users_UserId" FOREIGN KEY ("UserId") 
        REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- 4. Crear tabla EmailVerificationCodes (2FA por email)
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

-- 5. Crear índices para PasswordResetToken
CREATE INDEX IF NOT EXISTS "IX_PasswordResetToken_UserId" 
    ON "PasswordResetToken" ("UserId");

CREATE INDEX IF NOT EXISTS "IX_PasswordResetToken_Token" 
    ON "PasswordResetToken" ("Token");

-- 6. Crear índices para EmailVerificationToken
CREATE INDEX IF NOT EXISTS "IX_EmailVerificationToken_UserId" 
    ON "EmailVerificationToken" ("UserId");

CREATE INDEX IF NOT EXISTS "IX_EmailVerificationToken_Token" 
    ON "EmailVerificationToken" ("Token");

-- 7. Crear índices para EmailVerificationCodes
CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_UserId" 
    ON "EmailVerificationCodes" ("UserId");

CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_Code" 
    ON "EmailVerificationCodes" ("Code");

CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_UserId_Code_IsUsed" 
    ON "EmailVerificationCodes" ("UserId", "Code", "IsUsed");

-- Verificar creación
SELECT table_name 
FROM information_schema.tables 
WHERE table_name IN ('PasswordResetToken', 'EmailVerificationToken', 'EmailVerificationCodes')
ORDER BY table_name;

-- Mostrar mensaje
DO $$ 
BEGIN 
    RAISE NOTICE 'Tablas creadas exitosamente: PasswordResetToken, EmailVerificationToken, EmailVerificationCodes';
END $$;

