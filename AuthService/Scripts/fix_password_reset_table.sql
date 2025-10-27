-- Script para arreglar la tabla PasswordResetToken
-- Ejecuta esto en PostgreSQL

-- Agregar valor por defecto a IsUsed si no lo tiene
ALTER TABLE "PasswordResetToken" 
    ALTER COLUMN "IsUsed" SET DEFAULT false;

-- Agregar valor por defecto a CreatedAt si no lo tiene
ALTER TABLE "PasswordResetToken" 
    ALTER COLUMN "CreatedAt" SET DEFAULT now();

-- Hacer que IsUsed sea NOT NULL si no lo es
ALTER TABLE "PasswordResetToken" 
    ALTER COLUMN "IsUsed" SET NOT NULL;

-- Verificar la estructura
SELECT 
    column_name,
    data_type,
    column_default,
    is_nullable
FROM 
    information_schema.columns 
WHERE 
    table_name = 'PasswordResetToken'
    AND column_name IN ('IsUsed', 'CreatedAt')
ORDER BY 
    ordinal_position;

