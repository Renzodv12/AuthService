-- Script para crear la tabla EmailVerificationCodes
-- Ejecutar en PostgreSQL: psql -U postgres -d erp -f create_email_verification_codes.sql

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

CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_UserId" ON "EmailVerificationCodes" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_Code" ON "EmailVerificationCodes" ("Code");
CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_UserId_Code_IsUsed" ON "EmailVerificationCodes" ("UserId", "Code", "IsUsed");

COMMENT ON TABLE "EmailVerificationCodes" IS 'Códigos de verificación de dos factores enviados por email';

