-- categories 테이블 생성
CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

-- 초기 데이터 주입
INSERT INTO categories (id, name) VALUES
(1, '학사/수업'),
(2, '행사'),
(3, '장학금/등록금/지원금'),
(4, '취업/창업/경력'),
(5, '국제/교환/유학생'),
(6, '비교과/자기계발'),
(7, '생활/복지/시설'),
(8, '봉사');

-- source_items 테이블 생성
CREATE TABLE source_items (
    id BIGSERIAL PRIMARY KEY,
    source_type VARCHAR(50) NOT NULL, -- instagram, kw_notice, kw_academic
    source_id VARCHAR(255) NOT NULL,
    source_url TEXT,
    content_type VARCHAR(20) NOT NULL, -- html, json, text
    raw_content TEXT NOT NULL,
    crawled_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    parse_status VARCHAR(20) NOT NULL DEFAULT 'pending', -- pending / parsed / failed
    parse_error TEXT,
    UNIQUE(source_type, source_id)
);

CREATE INDEX idx_source_items_parse_status ON source_items(parse_status);

-- events 테이블 생성
CREATE TABLE events (
    id BIGSERIAL PRIMARY KEY,
    source_item_id BIGINT NOT NULL REFERENCES source_items(id),
    title VARCHAR(255) NOT NULL,
    body TEXT,
    start_dt TIMESTAMPTZ NOT NULL,
    end_dt TIMESTAMPTZ,
    is_all_day BOOLEAN NOT NULL DEFAULT FALSE,
    notice_dt TIMESTAMPTZ,
    external_link TEXT,
    category_id INTEGER NOT NULL REFERENCES categories(id),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_events_source_item_id ON events(source_item_id);
CREATE INDEX idx_events_start_dt ON events(start_dt);
CREATE INDEX idx_events_category_id ON events(category_id);
