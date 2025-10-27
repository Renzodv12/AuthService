# Cómo Ejecutar la Migración Manualmente

## 👉 Opción 1: Usando psql desde la línea de comandos

```bash
# En Windows PowerShell o CMD
psql -U postgres -d erp -f AuthService\Scripts\create_email_verification_codes_completo.sql

# Con contraseña interactiva
# Te pedirá la contraseña de postgres cuando ejecutes este comando
```

## 👉 Opción 2: Conectarte directamente a PostgreSQL y ejecutar el script

### Paso 1: Conectarte a PostgreSQL

```bash
psql -U postgres -d erp
```

### Paso 2: Pegar y ejecutar el siguiente código SQL:

```sql
-- Habilitar extensión UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Crear la tabla EmailVerificationCodes
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

-- Crear índices
CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_UserId"
    ON "EmailVerificationCodes" ("UserId");

CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_Code"
    ON "EmailVerificationCodes" ("Code");

CREATE INDEX IF NOT EXISTS "IX_EmailVerificationCodes_UserId_Code_IsUsed"
    ON "EmailVerificationCodes" ("UserId", "Code", "IsUsed");

-- Verificar creación
SELECT table_name
FROM information_schema.tables
WHERE table_name = 'EmailVerificationCodes';
```

## 👉 Opción 3: Usando pgAdmin

1. Abre pgAdmin
2. Conéctate a tu servidor PostgreSQL
3. Expandir `erp` database
4. Click derecho en "Schemas" → "public" → "Tables"
5. Selecciona "Query Tool"
6. Abre el archivo `Scripts/create_email_verification_codes_completo.sql`
7. Ejecuta el script (F5 o botón Execute)

## 👉 Opción 4: Usando DBeaver u otro cliente

1. Abre DBeaver
2. Conéctate a tu base de datos `erp`
3. Abre SQL Editor
4. Copia y pega el código SQL del archivo:
   `AuthService/Scripts/create_email_verification_codes_completo.sql`
5. Ejecuta el script

## ✅ Verificar que la tabla se creó

Ejecuta este query para verificar:

```sql
-- Ver la estructura de la tabla
SELECT
    column_name,
    data_type,
    character_maximum_length,
    is_nullable
FROM
    information_schema.columns
WHERE
    table_name = 'EmailVerificationCodes'
ORDER BY
    ordinal_position;
```

Deberías ver 8 columnas: Id, UserId, Code, Email, ExpiresAt, IsUsed, CreatedAt, UsedAt

## 📍 Ubicación de los archivos

Los scripts SQL están en:

- `AuthService/Scripts/create_email_verification_codes_completo.sql` (completo)
- `AuthService/Scripts/create_email_verification_codes.sql` (simple)

## 🚨 Notas importantes

1. **Credenciales de PostgreSQL**:

   - Usuario: `postgres`
   - Base de datos: `erp`
   - Password: `a.123456`

2. **La tabla Users debe existir**: Este script asume que la tabla `Users` ya existe en tu base de datos.

3. **Si hay errores**: Si PostgreSQL dice que falta la extensión uuid-ossp, ejecuta primero:
   ```sql
   CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
   ```

## 🎯 Después de crear la tabla

Una vez que la tabla esté creada, puedes ejecutar la aplicación y usar:

- 2FA por email: `GET /security/2FA?typeAuth=EmailVerificationCode`
- Recuperación de contraseña: `POST /auth/request-password-reset`
