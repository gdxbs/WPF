-- This migration removes the now-redundant machine_id column.
-- The user_id column is now the sole identifier for a user's preferences.

ALTER TABLE public.user_preferences
DROP COLUMN IF EXISTS machine_id;
