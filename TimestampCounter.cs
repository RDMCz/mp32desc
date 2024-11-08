namespace mp32desc
{
    internal struct TimestampCounter(TimeSpan totalDuration = new())
    {
        public void Increment(double ms) => totalDuration += TimeSpan.FromMilliseconds(ms);

        public override readonly string ToString()
        {
            // <0:00 — 9:59> → <10:00 — 59:59> → <1:00:00 — 9:59:59> → <10:00:00 — 23:59:59> → <1:00:00:00 — 9:23:59:59> → ...
            string seconds = totalDuration.Seconds.ToString().PadLeft(2, '0');
            string minutes = totalDuration.Minutes.ToString();
            string hoursWithColon;
            string daysWithColon;
            if (totalDuration.Days == 0) {
                daysWithColon = "";
                if (totalDuration.Hours == 0) {
                    hoursWithColon = "";
                }
                else {
                    hoursWithColon = totalDuration.Hours.ToString() + ":";
                    minutes = minutes.PadLeft(2, '0');
                }
            }
            else {
                daysWithColon = totalDuration.Days.ToString() + ":";
                hoursWithColon = (totalDuration.Hours.ToString() + ":").PadLeft(3, '0');
                minutes = minutes.PadLeft(2, '0');
            }
            return $"{daysWithColon}{hoursWithColon}{minutes}:{seconds}";
        }
    }
}
