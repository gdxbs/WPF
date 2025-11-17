-- Step 1: Drop the old unique constraint on machine_id
ALTER TABLE public.user_preferences DROP CONSTRAINT IF EXISTS user_preferences_machine_id_key;

-- Step 2: Make the user_id column NOT NULL.
-- If you have existing rows with a null user_id, this might fail.
-- If it does, you can either clear the user_preferences table or manually assign user_ids.
ALTER TABLE public.user_preferences ALTER COLUMN user_id SET NOT NULL;

-- Step 3: Add a unique constraint to the user_id column.
-- This ensures each user has only one set of preferences.
ALTER TABLE public.user_preferences ADD CONSTRAINT user_preferences_user_id_key UNIQUE (user_id);
