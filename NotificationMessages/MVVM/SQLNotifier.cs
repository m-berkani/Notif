using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections.ObjectModel;

namespace NotificationMessages.MVVM
{
    public class SQLNotifier : IDisposable
    {

        public SqlCommand CurrentCommand { get; set; }
        private SqlConnection connection;
        public SqlConnection CurrentConnection
        {
            get
            {
                this.connection = this.connection ?? new SqlConnection(this.ConnectionString);
                return this.connection;
            }
        }
        public string ConnectionString
        {
            get
            {
                return @"Data Source=WIN-PEC9897DNVV\SQLEXPRESS;Initial Catalog=Notif;Integrated Security=True";
            }
        }

        public SQLNotifier()
        {
            SqlDependency.Start(this.ConnectionString);

        }
        private event EventHandler<SqlNotificationEventArgs> _newMessage;

        public event EventHandler<SqlNotificationEventArgs> NewMessage
        {
            add
            {
                this._newMessage += value;
            }
            remove
            {
                this._newMessage -= value;
            }
        }

        public virtual void OnNewMessage(SqlNotificationEventArgs notification)
        {
            if (this._newMessage != null)
                this._newMessage(this, notification);
        }
        public DataTable RegisterDependency()
        {

            this.CurrentCommand = new SqlCommand("Select [title],[description]from dbo.employee", this.CurrentConnection);
            this.CurrentCommand.Notification = null;


            SqlDependency dependency = new SqlDependency(this.CurrentCommand);
            dependency.OnChange += this.dependency_OnChange;


            if (this.CurrentConnection.State == ConnectionState.Closed)
                this.CurrentConnection.Open();
            try
            {

                DataTable dt = new DataTable();
                dt.Load(this.CurrentCommand.ExecuteReader(CommandBehavior.CloseConnection));
                return dt;
            }
            catch { return null; }

        }

        void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            SqlDependency dependency = sender as SqlDependency;

            dependency.OnChange -= new OnChangeEventHandler(dependency_OnChange);

            this.OnNewMessage(e);
        }
        public void Insert(string msgTitle, string description)
        {
            using (SqlConnection con = new SqlConnection(this.ConnectionString))
            {


                using (SqlCommand cmd = new SqlCommand("Insert into  employee values ('Moha','dev')", con))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;

                    //cmd.Parameters.AddWithValue("@title", msgTitle);
                    //cmd.Parameters.AddWithValue("@description", description);

                    con.Open();

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }

        }

        #region IDisposable Members

        public void Dispose()
        {
            SqlDependency.Stop(this.ConnectionString);
        }

        #endregion
    }
}
