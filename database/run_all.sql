-- Run this against a fresh, empty PostgreSQL database to build the entire
-- CommerceCore schema from scratch, in the correct dependency order:
--   psql -U postgres -d commercecore_db -f run_all.sql
\i 01_identity_and_stores.sql
\i 02_catalog.sql
\i 03_inventory.sql
\i 04_customers_orders.sql
\i 05_payments_marketing.sql
\i 06_cms.sql
\i 07_reference.sql
\i 08_media_notifications_system.sql
\i 99_seed_data.sql
