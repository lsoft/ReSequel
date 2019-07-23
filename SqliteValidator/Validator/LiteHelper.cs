using System;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace SqliteValidator.Validator
{
    /// <summary>
    /// Helper class to workaround this issue: https://github.com/aspnet/EntityFrameworkCore/issues/16647
    /// </summary>
    internal static class LiteHelper
    {
        public static void AddDummyParameters(this SqliteCommand command)
        {
            var db = command.Connection.Handle;

            var remainingSql = command.CommandText;
            while (!string.IsNullOrEmpty(remainingSql))
            {
                var rc = raw.sqlite3_prepare_v2(db, remainingSql, out var stmt, out remainingSql);
                SqliteException.ThrowExceptionForRC(rc, db);

                if (stmt.ptr == IntPtr.Zero)
                {
                    // Statement was empty, white space, or a comment
                    continue;
                }

                using (stmt)
                {
                    var count = raw.sqlite3_bind_parameter_count(stmt);
                    for (var i = 1; i <= count; i++)
                    {
                        var name = raw.sqlite3_bind_parameter_name(stmt, i);

                        // Add a dummy value
                        command.Parameters.AddWithValue(name, "dummy");
                    }
                }
            }
        }

    }
}