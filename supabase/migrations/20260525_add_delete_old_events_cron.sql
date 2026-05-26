-- 오래된 일정 자동 삭제 기능 추가
-- 기존 동일 이름의 cron job이 있으면 제거 후 다시 등록한다.

create extension if not exists pg_cron;

select cron.unschedule('delete-old-events-daily')
where exists (
  select 1
  from cron.job
  where jobname = 'delete-old-events-daily'
);

select cron.schedule(
  'delete-old-events-daily',
  '0 3 * * *',
  $$
    delete from public.events
    where coalesce(end_at, start_at) < now() - interval '6 months';
  $$
);
