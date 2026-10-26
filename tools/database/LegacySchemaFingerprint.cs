// PostgreSQL catalog inspection for safe adoption of pre-EF databases only.
// Executed through EF Core; never used for schema creation or business CRUD.
internal static class LegacySchemaFingerprint
{
    public const string Query = """
SELECT COALESCE(jsonb_agg(item ORDER BY item::text), '[]'::jsonb)::text AS "Value"
FROM (
    SELECT jsonb_build_object('table', c.relname, 'column', a.attname,
        'type', format_type(a.atttypid, a.atttypmod), 'required', a.attnotnull,
        'identity', a.attidentity, 'default', pg_get_expr(d.adbin, d.adrelid)) AS item
    FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace
    JOIN pg_attribute a ON a.attrelid=c.oid AND a.attnum>0 AND NOT a.attisdropped
    LEFT JOIN pg_attrdef d ON d.adrelid=c.oid AND d.adnum=a.attnum
    WHERE n.nspname={0} AND c.relkind='r' AND c.relname<>'__EFMigrationsHistory'
    UNION ALL
    SELECT jsonb_build_object('table', c.relname, 'constraint', con.conname,
        'definition', pg_get_constraintdef(con.oid, true))
    FROM pg_constraint con JOIN pg_class c ON c.oid=con.conrelid JOIN pg_namespace n ON n.oid=c.relnamespace
    WHERE n.nspname={0} AND c.relname<>'__EFMigrationsHistory'
    UNION ALL
    SELECT jsonb_build_object('table', tablename, 'index', indexname, 'definition', indexdef)
    FROM pg_indexes WHERE schemaname={0} AND tablename<>'__EFMigrationsHistory'
    UNION ALL
    SELECT jsonb_build_object('view', viewname, 'definition', definition)
    FROM pg_views WHERE schemaname={0}
) schema_items
""";
}
