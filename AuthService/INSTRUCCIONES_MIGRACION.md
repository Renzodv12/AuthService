# Instrucciones para Aplicar Migración

## Opción 1: Usando Entity Framework (Recomendado)

Cuando la aplicación no esté en ejecución:

```bash
cd AuthService
dotnet ef migrations add AddEmailVerificationCodes
dotnet ef database update
```

Si hay un proceso bloqueado, cierra la aplicación y vuelve a intentar.

## Opción 2: Ejecutar Script SQL Directamente

Si prefieres ejecutar el script SQL directamente en PostgreSQL:

```bash
cd AuthService
psql -U postgres -d erp -f Scripts/create_email_verification_codes.sql
```

O desde pgAdmin o cualquier cliente PostgreSQL, ejecutar el contenido de:
`AuthService/Scripts/create_email_verification_codes.sql`

## Opción 3: Desde Visual Studio

1. Abre el Package Manager Console en Visual Studio
2. Selecciona el proyecto `AuthService` como proyecto predeterminado
3. Ejecuta:

```
Add-Migration AddEmailVerificationCodes
Update-Database
```

## Verificación

Después de aplicar la migración, verifica que la tabla se creó correctamente:

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_name = 'EmailVerificationCodes';
```

La tabla debe existir con las siguientes columnas:

- Id (uuid)
- UserId (uuid)
- Code (varchar(10))
- Email (varchar(100))
- ExpiresAt (timestamp)
- IsUsed (boolean)
- CreatedAt (timestamp)
- UsedAt (timestamp, nullable)
