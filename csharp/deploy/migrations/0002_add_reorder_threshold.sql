-- Stockroom creates its schema on first start, so a new database already has this column.
-- Run this once against a database created before 1.1.0.
ALTER TABLE products ADD COLUMN ReorderThreshold INTEGER NOT NULL DEFAULT 0;
