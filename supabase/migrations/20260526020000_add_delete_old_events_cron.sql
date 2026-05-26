-- 오래된 일정 자동 삭제 기능 추가
-- 매일 03:00에 실행되며, 일정 종료 시점 기준 6개월이 지난 일정을 삭제한다.
-- end_dt가 없는 일정은 start_dt를 기준으로 판단한다.

CREATE EXTENSION IF NOT EXISTS pg_cron;

SELECT cron.unschedule('delete-old-events-daily')
WHERE EXISTS (
    SELECT 1
    FROM cron.job
    WHERE jobname = 'delete-old-events-daily'
);

SELECT cron.schedule(
    'delete-old-events-daily',
    '0 3 * * *',
    $$
        DELETE FROM public.events
        WHERE COALESCE(end_dt, start_dt) < NOW() - INTERVAL '6 months';
    $$
);
