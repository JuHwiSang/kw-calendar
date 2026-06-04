-- Ensure event categories used by parse_events exist.
-- Existing IDs:
-- 7 = 생활/복지/시설
-- 8 = 봉사
-- New ID:
-- 9 = 기타

INSERT INTO categories (id, name)
VALUES
    (7, '생활/복지/시설'),
    (8, '봉사'),
    (9, '기타')
ON CONFLICT (id) DO UPDATE
SET name = EXCLUDED.name;

-- Keep categories id sequence in sync after explicit id insertion.
SELECT setval('categories_id_seq', (SELECT MAX(id) FROM categories));
