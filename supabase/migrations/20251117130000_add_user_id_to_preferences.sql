ALTER TABLE user_preferences
ADD COLUMN user_id UUID REFERENCES users(id);
