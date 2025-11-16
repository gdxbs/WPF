/*
  # Create user preferences and settings schema

  1. New Tables
    - `user_preferences`
      - `id` (uuid, primary key)
      - `machine_id` (text, unique - identifies machine/app instance)
      - `theme_mode` (text - 'light' or 'dark')
      - `accent_color` (text - hex color code)
      - `font_size` (integer - 10-18)
      - `window_transparency` (integer - 0-100)
      - `background_image_path` (text - file path or URL)
      - `sound_enabled` (boolean)
      - `notification_volume` (integer - 0-100)
      - `message_notification_enabled` (boolean)
      - `join_leave_notification_enabled` (boolean)
      - `error_notification_enabled` (boolean)
      - `auto_connect_enabled` (boolean)
      - `last_connection_host` (text)
      - `last_connection_port` (integer)
      - `created_at` (timestamp)
      - `updated_at` (timestamp)
    
    - `notification_sounds`
      - `id` (uuid, primary key)
      - `preferences_id` (uuid, foreign key to user_preferences)
      - `event_type` (text - 'message', 'join_leave', 'error', 'custom')
      - `sound_file_path` (text - path to custom sound file)
      - `created_at` (timestamp)
      - `updated_at` (timestamp)

  2. Security
    - Enable RLS on both tables
    - Add policies allowing public access (WPF app doesn't have user auth)
    
  3. Indexes
    - Index on machine_id for fast preference lookups
    - Index on event_type for sound file queries
*/

CREATE TABLE IF NOT EXISTS user_preferences (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  machine_id text UNIQUE NOT NULL,
  theme_mode text DEFAULT 'light',
  accent_color text DEFAULT '#0078D4',
  font_size integer DEFAULT 12,
  window_transparency integer DEFAULT 100,
  background_image_path text,
  sound_enabled boolean DEFAULT true,
  notification_volume integer DEFAULT 70,
  message_notification_enabled boolean DEFAULT true,
  join_leave_notification_enabled boolean DEFAULT true,
  error_notification_enabled boolean DEFAULT true,
  auto_connect_enabled boolean DEFAULT false,
  last_connection_host text,
  last_connection_port integer,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now()
);

CREATE TABLE IF NOT EXISTS notification_sounds (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  preferences_id uuid NOT NULL REFERENCES user_preferences(id) ON DELETE CASCADE,
  event_type text NOT NULL,
  sound_file_path text,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now()
);

ALTER TABLE user_preferences ENABLE ROW LEVEL SECURITY;
ALTER TABLE notification_sounds ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Allow all reads on user_preferences"
  ON user_preferences FOR SELECT
  TO public
  USING (true);

CREATE POLICY "Allow all inserts on user_preferences"
  ON user_preferences FOR INSERT
  TO public
  WITH CHECK (true);

CREATE POLICY "Allow all updates on user_preferences"
  ON user_preferences FOR UPDATE
  TO public
  USING (true)
  WITH CHECK (true);

CREATE POLICY "Allow all deletes on user_preferences"
  ON user_preferences FOR DELETE
  TO public
  USING (true);

CREATE POLICY "Allow all reads on notification_sounds"
  ON notification_sounds FOR SELECT
  TO public
  USING (true);

CREATE POLICY "Allow all inserts on notification_sounds"
  ON notification_sounds FOR INSERT
  TO public
  WITH CHECK (true);

CREATE POLICY "Allow all updates on notification_sounds"
  ON notification_sounds FOR UPDATE
  TO public
  USING (true)
  WITH CHECK (true);

CREATE POLICY "Allow all deletes on notification_sounds"
  ON notification_sounds FOR DELETE
  TO public
  USING (true);

CREATE INDEX idx_machine_id ON user_preferences(machine_id);
CREATE INDEX idx_preferences_id ON notification_sounds(preferences_id);
CREATE INDEX idx_event_type ON notification_sounds(event_type);
