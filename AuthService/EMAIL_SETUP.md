# Email Configuration - AuthService

## Configuración

Este proyecto utiliza configuración de email para:

- Verificación de email al registrarse
- Recuperación de contraseña ("Olvidé mi contraseña")
- Códigos de verificación de dos factores (2FA) por email

## Configuración de Email

### Configuración en appsettings.json

Las configuraciones de email se encuentran en `appsettings.json` y `appsettings.Development.json`.

**Para desarrollo (appsettings.Development.json):**

- Usuario SMTP: unidatesis@gmail.com
- Contraseña de aplicación: fahi snuv ymnn haez
- Email de prueba: renzodragotto50@gmail.com

### Variables de Entorno

Para producción, puedes usar las siguientes variables de entorno:

```bash
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_SMTP_USERNAME=unidatesis@gmail.com
EMAIL_SMTP_PASSWORD=fahi snuv ymnn haez
EMAIL_FROM_EMAIL=unidatesis@gmail.com
EMAIL_FROM_NAME=AuthService
EMAIL_BASE_URL=http://localhost:5022
TEST_EMAIL=renzodragotto50@gmail.com
```

### Redirección de Email para Pruebas

Para enviar todos los emails a una dirección de prueba, establece la variable de entorno `TEST_EMAIL`:

```bash
export TEST_EMAIL=renzodragotto50@gmail.com
```

O en Windows PowerShell:

```powershell
$env:TEST_EMAIL="renzodragotto50@gmail.com"
```

Cuando `TEST_EMAIL` está configurado, todos los emails se enviarán a esa dirección, pero el cuerpo del email incluirá el destinatario original para referencia.

## Características Implementadas

### 1. Verificación de Email

Al registrarse, el usuario recibe un email con un enlace de verificación:

- Endpoint: `POST /auth/verify-email`
- Duración: 24 horas

### 2. Recuperación de Contraseña

Si el usuario olvida su contraseña:

- Endpoint para solicitar: `POST /auth/request-password-reset`
- Endpoint para restablecer: `POST /auth/reset-password`
- Duración del token: 1 hora
- Se invalida el token anterior cuando se solicita uno nuevo

### 3. Verificación de Dos Factores por Email

#### Configuración de 2FA por Email

1. Solicitar código de 2FA:
   - Endpoint: `GET /security/2FA?typeAuth=EmailVerificationCode`
   - Requiere autenticación (Bearer token)
   - Genera un código de 6 dígitos
   - Envía el código por email
   - Duración: 10 minutos

#### Validación de Código de 2FA

2. Validar código de 2FA:
   - Endpoint: `POST /security/2FA`
   - Body: `{ "secretKey": "123456", "typeAuth": "EmailVerificationCode" }`
   - Requiere autenticación (Bearer token)
   - Marca el código como usado
   - Retorna un nuevo JWT con 2FA verificado

## Flujo de 2FA por Email

```
1. Usuario solicita código 2FA (GET /security/2FA)
   ↓
2. Sistema genera código de 6 dígitos
   ↓
3. Sistema guarda código en BD (válido 10 minutos)
   ↓
4. Sistema envía email con el código
   ↓
5. Usuario ingresa el código (POST /security/2FA)
   ↓
6. Sistema valida el código
   ↓
7. Sistema marca código como usado
   ↓
8. Sistema genera nuevo JWT con 2FA verificado
```

## Configuración en appsettings.Development.json

Actualmente configurado para desarrollo local con:

- Email de envío: unidatesis@gmail.com
- Email de prueba: renzodragotto50@gmail.com
- Todos los emails se redirigen a la dirección de prueba

## Notas de Seguridad

1. **Contraseña de Aplicación**: Gmail requiere una "App Password" para aplicaciones externas
2. **Tokens de Seguridad**: Los tokens tienen expiración automática
3. **Invalidación**: Los tokens anteriores se invalidan cuando se solicita uno nuevo
4. **Single Use**: Los códigos de 2FA son de un solo uso

## Troubleshooting

### Los emails no se envían

1. Verifica las credenciales en `appsettings.Development.json`
2. Verifica que `TEST_EMAIL` esté configurado (si quieres redirección)
3. Verifica los logs en la consola
4. Verifica que Hangfire esté corriendo (para envío en background)

### Códigos de 2FA no llegan

1. Verifica la cola de Hangfire en `/hangfire`
2. Revisa los logs para errores de SMTP
3. Verifica que el email esté registrado en el sistema

## Migraciones

Para aplicar los cambios de base de datos:

```bash
dotnet ef migrations add AddEmailVerificationCodeEntity
dotnet ef database update
```
