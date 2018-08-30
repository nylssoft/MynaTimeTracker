/*
    Myna Time Tracker
    Copyright (C) 2018 Niels Stockfleth

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace TimeTracker
{
    public class Database : IDisposable
    {
        private SQLiteConnection con;

        public void Dispose()
        {
            con?.Dispose();
        }

        public void Open(string filename)
        {
            var sb = new SQLiteConnectionStringBuilder() { DataSource = filename };
            con = new SQLiteConnection(sb.ToString());
            con.Open();
            Init(con);
        }

        private void Init(SQLiteConnection con)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "PRAGMA foreign_keys = ON;";
                cmd.ExecuteNonQuery();
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS project " +
                    "(rowid INTEGER NOT NULL," +
                    " name TEXT NOT NULL," +
                    " PRIMARY KEY(rowid)," +
                    " CONSTRAINT project_unique UNIQUE (name));";
                cmd.ExecuteNonQuery();
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS worktime " +
                    "(starttime INTEGER NOT NULL," +
                    " endtime INTEGER NOT NULL," +
                    " projectid INTEGER NOT NULL," +
                    " description TEXT, " +
                    " FOREIGN KEY(projectid) REFERENCES project(rowid));";
                cmd.ExecuteNonQuery();
                cmd.CommandText =
                    "CREATE INDEX IF NOT EXISTS worktime_index1 ON worktime (starttime);";
                cmd.ExecuteNonQuery();
                cmd.CommandText =
                    "CREATE INDEX IF NOT EXISTS worktime_index2 ON worktime (endtime);";
                cmd.ExecuteNonQuery();
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS worktimeperday " +
                    "(day INTEGER NOT NULL," +
                    " worked REAL NOT NULL," + 
                    " required REAL NOT NULL," +
                    " PRIMARY KEY(day));";
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateWorkTimePerDay(DateTime dt, double hours)
        {
            dt = dt.GetDayDateTime();
            using (var cmd = new SQLiteCommand(con))
            {
                // SQLite 3.24
                cmd.CommandText =
                    "INSERT INTO worktimeperday(day,hours) VALUES(@p1,@p2)" +
                    " ON CONFLICT(day) DO UPDATE SET hours=excluded.hours";
                /* previous versions
                bool exists = false;
                cmd.CommandText = "SELECT 1 FROM worktimeperday WHERE day=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", dt.GetUnixTimeSeconds()));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        exists = true;
                    }
                }
                cmd.Parameters.Clear();
                if (!exists)
                {
                    cmd.CommandText = "INSERT INTO worktimeperday(day,hours) VALUES(@p1,@p2)";
                }
                else
                {
                    cmd.CommandText = "UPDATE worktimeperday SET hours=@p2 WHERE day=@p1";
                }
                */
                cmd.Parameters.Add(new SQLiteParameter("@p1", dt.GetUnixTimeSeconds()));
                cmd.Parameters.Add(new SQLiteParameter("@p2", hours));
                cmd.ExecuteNonQuery();
            }
        }

        public double GetTotalHours(DateTime dtfrom, DateTime dtto)
        {
            dtfrom = dtfrom.GetDayDateTime();
            dtto = dtto.GetDayDateTime();
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText =
                    "SELECT SUM(hours) FROM worktimeperday WHERE day>=@p1 AND day<=@p2";
                cmd.Parameters.Add(new SQLiteParameter("@p1", dtfrom.GetUnixTimeSeconds()));
                cmd.Parameters.Add(new SQLiteParameter("@p2", dtto.GetUnixTimeSeconds()));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        return reader.GetDouble(0);
                    }
                }
            }
            return 0.0;
        }

        public DateTime? GetFirstStartTime()
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "SELECT MIN(StartTime) FROM worktime";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(0)).DateTime.ToLocalTime();
                    }
                }
            }
            return null;
        }

        public void InsertWorkTime(WorkTime wt)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "INSERT INTO worktime VALUES(@p1,@p2,@p3,@p4)";
                cmd.Parameters.Add(new SQLiteParameter("@p1", ((DateTimeOffset)wt.StartTime.ToUniversalTime()).ToUnixTimeSeconds()));
                cmd.Parameters.Add(new SQLiteParameter("@p2", ((DateTimeOffset)wt.EndTime.ToUniversalTime()).ToUnixTimeSeconds()));
                cmd.Parameters.Add(new SQLiteParameter("@p3", wt.Project.Id));
                cmd.Parameters.Add(new SQLiteParameter("@p4", wt.Description));
                cmd.ExecuteNonQuery();
                wt.Id = con.LastInsertRowId;
            }
        }

        public void DeleteWorkTime(WorkTime wt)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "DELETE FROM worktime WHERE rowid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", wt.Id));
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateWorkTime(WorkTime wt)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "UPDATE worktime SET starttime=@p1,endtime=@p2,projectid=@p3,description=@p4 WHERE rowid=@p5";
                cmd.Parameters.Add(new SQLiteParameter("@p1", ((DateTimeOffset)wt.StartTime.ToUniversalTime()).ToUnixTimeSeconds()));
                cmd.Parameters.Add(new SQLiteParameter("@p2", ((DateTimeOffset)wt.EndTime.ToUniversalTime()).ToUnixTimeSeconds()));
                cmd.Parameters.Add(new SQLiteParameter("@p3", wt.Project.Id));
                cmd.Parameters.Add(new SQLiteParameter("@p4", wt.Description));
                cmd.Parameters.Add(new SQLiteParameter("@p5", wt.Id));
                cmd.ExecuteNonQuery();
            }
        }

        public Project SelectProjectByName(String name)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "SELECT rowid, name FROM project WHERE name = @p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", name));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        return new Project { Id = reader.GetInt64(0), Name = reader.GetString(1)};
                    }
                }
            }
            return null;
        }

        public Project InsertProject(string name)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "INSERT INTO project (name) VALUES(@p1)";
                cmd.Parameters.Add(new SQLiteParameter("@p1", name));
                cmd.ExecuteNonQuery();
                return new Project { Id = con.LastInsertRowId, Name = name };
            }
        }

        public void DeleteProject(Project project)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "DELETE FROM project WHERE rowid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", project.Id));
                cmd.ExecuteNonQuery();
            }
        }

        public void RenameProject(Project project, string newName)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "UPDATE project SET name=@p1 WHERE rowid=@p2";
                cmd.Parameters.Add(new SQLiteParameter("@p1", newName));
                cmd.Parameters.Add(new SQLiteParameter("@p2", project.Id));
                cmd.ExecuteNonQuery();
                project.Name = newName;
            }
        }

        public List<Project> SelectProjectInUse()
        {
            var ret = new List<Project>();
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText =
                    "SELECT DISTINCT prj.rowid, prj.name FROM project prj, worktime wt" +
                    " WHERE prj.rowid = wt.projectid ORDER BY prj.name";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ret.Add(new Project { Id = reader.GetInt64(0), Name = reader.GetString(1) });
                        }
                    }
                }
            }
            return ret;
        }

        public List<Project> SelectAllProjects()
        {
            var ret = new List<Project>();
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "SELECT rowid, name FROM project ORDER BY name";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ret.Add(new Project { Id = reader.GetInt64(0), Name = reader.GetString(1) });
                        }
                    }
                }
            }
            return ret;
        }

        public List<WorkTime> SelectWorkTimes(DateTime day)
        {
            var st = new DateTime(day.Year, day.Month, day.Day, 0, 0, 0).ToUniversalTime();
            var et = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59).ToUniversalTime();
            var ret = new List<WorkTime>();
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText =
                    "SELECT wt.rowid, starttime, endtime, p.rowid, p.name, description "+
                    "FROM worktime wt, project p " +
                    "WHERE starttime >= @p1 AND starttime <= @p2 AND wt.projectid = p.rowid";
                cmd.Parameters.Add(new SQLiteParameter("@p1", ((DateTimeOffset)st).ToUnixTimeSeconds()));
                cmd.Parameters.Add(new SQLiteParameter("@p2", ((DateTimeOffset)et).ToUnixTimeSeconds()));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var wt = new WorkTime
                            {
                                Id = reader.GetInt64(0),
                                StartTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(1)).DateTime.ToLocalTime(),
                                EndTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2)).DateTime.ToLocalTime(),
                                Project = new Project { Id = reader.GetInt64(3), Name = reader.GetString(4)},
                                Description = reader.GetString(5)
                            };
                            ret.Add(wt);
                        }
                    }
                }
            }
            return ret;
        }
    }
}
