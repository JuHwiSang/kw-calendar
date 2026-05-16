-- RLS 활성화
ALTER TABLE categories   ENABLE ROW LEVEL SECURITY;
ALTER TABLE source_items ENABLE ROW LEVEL SECURITY;
ALTER TABLE events       ENABLE ROW LEVEL SECURITY;

-- ============================================================
-- categories: anon/authenticated → SELECT only, service_role → ALL
-- ============================================================
CREATE POLICY "categories_read_anon"
    ON categories FOR SELECT
    TO anon, authenticated
    USING (true);

CREATE POLICY "categories_all_service"
    ON categories FOR ALL
    TO service_role
    USING (true)
    WITH CHECK (true);

-- ============================================================
-- source_items: anon/authenticated → 접근 불가, service_role → ALL
-- (RLS 활성화 후 기본 DENY이므로 anon/authenticated 정책은 불필요)
-- ============================================================
CREATE POLICY "source_items_all_service"
    ON source_items FOR ALL
    TO service_role
    USING (true)
    WITH CHECK (true);

-- ============================================================
-- events: anon/authenticated → SELECT only, service_role → ALL
-- ============================================================
CREATE POLICY "events_read_anon"
    ON events FOR SELECT
    TO anon, authenticated
    USING (true);

CREATE POLICY "events_all_service"
    ON events FOR ALL
    TO service_role
    USING (true)
    WITH CHECK (true);

-- ============================================================
-- postgres 롤: 세 테이블 모두 GRANT ALL (슈퍼유저이나 명시적으로 부여)
-- ============================================================
GRANT ALL ON TABLE categories   TO postgres;
GRANT ALL ON TABLE source_items TO postgres;
GRANT ALL ON TABLE events       TO postgres;
