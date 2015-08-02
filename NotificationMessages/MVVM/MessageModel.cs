using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Data;
using System.Windows.Input;
using System.Data.SqlClient;

namespace NotificationMessages.MVVM
{
    public class MessageModel : INotifyPropertyChanged
    {
        private ObservableCollection<Message> messages = null;
        public ObservableCollection<Message> Messages
        {
            get
            {
                messages = messages ?? new ObservableCollection<Message>();
                return messages;
            }
        }

        private ICommand insertMessage;

        public ICommand InsertMessage
        {
            get
            {
                this.insertMessage = this.insertMessage ?? new DelegateCommand(this.ExecuteInsert);
                return this.insertMessage;
            }
        }
        
        public Dispatcher UIDispatcher { get; set; }

        public SQLNotifier Notifier { get; set; }
        public MessageModel(Dispatcher uidispatcher)
        {
            this.UIDispatcher = uidispatcher;
            this.Notifier = new SQLNotifier();

            this.Notifier.NewMessage += new EventHandler<SqlNotificationEventArgs>(notifier_NewMessage);
            DataTable dt = this.Notifier.RegisterDependency();


            this.LoadMessage(dt);
        }

        private string title;
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
                this.OnPropertyChanged("Title");
            }
        }
        private string description;
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
                this.OnPropertyChanged("Description");
            }
        }
        public void ExecuteInsert()
        {
            this.Notifier.Insert(this.Title, this.Description);
            this.Title = string.Empty;
            this.Description = string.Empty;
        }
        private void LoadMessage(DataTable dt)
        {
            
            this.UIDispatcher.BeginInvoke((Action)delegate()
            {
                if (dt != null)
                {
                    this.Messages.Clear();

                    foreach (DataRow drow in dt.Rows)
                    {
                        Message msg = new Message
                        {
                            Id = Convert.ToString(0),
                            Title = drow["title"] as string,
                            Description = drow["description"] as string
                        };
                        this.Messages.Add(msg);
                    }
                }
            });
        }
        void notifier_NewMessage(object sender, SqlNotificationEventArgs e)
        {
            this.LoadMessage(this.Notifier.RegisterDependency());
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
