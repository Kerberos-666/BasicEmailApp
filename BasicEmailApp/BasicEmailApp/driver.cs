﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace BasicEmailApp
{
    public partial class driver : Form
    {
        //gets the e-mail used after successfully logging in and saves it in the driver for future use.
        static Form loginForm = Application.OpenForms["login"];
        public string g_user_email = ((login)loginForm).s_email;
        string g_user_id;
        string connectionString = "Data Source=DESKTOP-QF9IM65\\TESTSQL;Initial Catalog=EmailProject;Integrated Security=True";
        public driver()
        {
            InitializeComponent();
            // get userID from email
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            if (conn.State == ConnectionState.Open)
            {
                string user_id_query = "select USERID from [USER] where EMAIL = '" + g_user_email + "'";
                SqlCommand validateCmd = new SqlCommand(user_id_query, conn);
                g_user_id = Convert.ToString(validateCmd.ExecuteScalar());
            }
            conn.Close();
        }
   
        private void driver_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        public void refreshInbox()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            if (conn.State == ConnectionState.Open)
            {
                //get table of all e-mails sent to this user THAT ARE NOT ARCHIVED
                string get_emails_to_account = "select * from EMAIL where EMAIL.RECEIVERID = " + g_user_id + " AND EMAIL.ARCHIVED = 0";

                //gets the sender's name and display the table properly
                string show_emails_with_sender = "select Q.BODY, Q.SENDERID, Q.EMAILID , [USER].FIRSTNAME as [Sent by], [SUBJECT] as [Subject], [DATE] as [Date] from [USER] inner join (" + get_emails_to_account + ") as Q";
                string show_condition = " on [USER].USERID = Q.SENDERID order by [DATE] DESC";

                SqlDataAdapter sqlAdpt = new SqlDataAdapter(show_emails_with_sender + show_condition, conn);

                //inbox_data_table has all the rows from executing show_emails_with_sender + show_condition
                DataTable inbox_data_table = new DataTable();
                sqlAdpt.Fill(inbox_data_table);
                inbox_data_view.DataSource = inbox_data_table;
                inbox_data_view.Columns["EMAILID"].Visible = false;
                inbox_data_view.Columns["SENDERID"].Visible = false;
                inbox_data_view.Columns["BODY"].Visible = false;
                logged_in_as.Text = "logged in as <" + g_user_email + ">";


                //updates the archived tab

                //all e-mails sent to this user THAT ARE ARCHIVED
                get_emails_to_account = "select * from EMAIL where EMAIL.RECEIVERID = " + g_user_id + " AND EMAIL.ARCHIVED = 1";
                show_emails_with_sender = "select Q.BODY, Q.SENDERID, Q.EMAILID , [USER].FIRSTNAME as [Sent by], [SUBJECT] as [Subject], [DATE] as [Date] from [USER] inner join (" + get_emails_to_account + ") as Q";
                show_condition = " on [USER].USERID = Q.SENDERID order by [DATE] DESC";
                //gets the sender's name and displays the table properly
                SqlDataAdapter ArchSqlAdpt = new SqlDataAdapter(show_emails_with_sender + show_condition, conn);
                DataTable archive_data_table = new DataTable();
                ArchSqlAdpt.Fill(archive_data_table);
                archive_data_view.DataSource = archive_data_table;
                archive_data_view.Columns["EMAILID"].Visible = false;
                archive_data_view.Columns["SENDERID"].Visible = false;
                archive_data_view.Columns["BODY"].Visible = false;
            }
            conn.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            refreshInbox();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void driver_Load(object sender, EventArgs e)
        {
            logged_in_as.Text = "logged in as <" + g_user_email + ">";
            this.Text = this.Text + " | " + g_user_email;
            //loads data into inbox_grid_view
            refreshInbox();
            inbox_data_view.MultiSelect = false;
            archive_data_view.MultiSelect = false;
            //TODO: SQL querIES to load all the data needed for all tabs
            //[DONE] TODO: Load messages and their senders into the inbox_data_view and sort them from most recent to oldest
            //TODO: Load folders in their respective way in the Folders tab
            //TODO: Load mailing list in its respective way in the Mailing List tab
            //[DONE] TODO: Load archived messages into archived inbox
        }


        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            if (inbox_data_view.CurrentRow == null)
            {
                status_label.Text = "select Email to delete.";
            }
            else
            {
                string selected_email = inbox_data_view.CurrentRow.Cells["EMAILID"].Value.ToString();
                string delete_selected_query = "delete from EMAIL where EMAILID =" + selected_email;
                SqlCommand validateCmd = new SqlCommand(delete_selected_query, conn);
                validateCmd.ExecuteNonQuery();
                status_label.Text = "delete successful.";
                refreshInbox();
            }
            conn.Close();
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            edit_account editAccForm = new edit_account();
            editAccForm.ShowDialog();
        }

        private void send_button_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            send_new sendForm = new send_new();
            sendForm.ShowDialog();
        }

        private void view_button_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (inbox_data_view.CurrentRow == null)
            {
                status_label.Text = "select Email to view.";
            }
            else
            {
                string selected_email = inbox_data_view.CurrentRow.Cells["EMAILID"].Value.ToString();
                view_email View = new view_email(selected_email);
                View.ShowDialog();
            }
        }

        private void inbox_data_view_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //selects the entire row instead of individual cells
            inbox_data_view.CurrentRow.Selected = true;
        }



        private void delete_archived_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            if (archive_data_view.CurrentRow == null)
            {
                status_label.Text = "select Email to delete.";
            }
            else
            {
                string selected_email = archive_data_view.CurrentRow.Cells["EMAILID"].Value.ToString();
                string delete_selected_query = "delete from EMAIL where EMAILID =" + selected_email;
                SqlCommand validateCmd = new SqlCommand(delete_selected_query, conn);
                validateCmd.ExecuteNonQuery();
                status_label.Text = "delete successful.";
                refreshInbox();
            }
            conn.Close();
        }

        private void view_archived_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (archive_data_view.CurrentRow == null)
            {
                status_label.Text = "select Email to view.";
            }
            else
            {
                string selected_email = archive_data_view.CurrentRow.Cells["EMAILID"].Value.ToString();
                view_email View = new view_email(selected_email);
                View.ShowDialog();
            }
        }

        private void archive_data_view_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            archive_data_view.CurrentRow.Selected = true; 
        }

        private void archive_button_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (inbox_data_view.SelectedRows == null) return;
            
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            if (inbox_data_view.SelectedRows.Count > 0)
            {
                for (int i = 0; i < inbox_data_view.SelectedRows.Count; i++)
                {
                    string selected_email = inbox_data_view.SelectedRows[i].Cells["EMAILID"].Value.ToString();
                    string update_query_selected = "UPDATE [EMAIL] SET ARCHIVED = 1 WHERE EMAILID = " + selected_email;
                    SqlCommand validateCmd = new SqlCommand(update_query_selected, conn);
                    validateCmd.ExecuteNonQuery();
                    refreshInbox();
                }
            }
            conn.Close();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (archive_data_view.SelectedRows == null) return;

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            if (archive_data_view.SelectedRows.Count > 0)
            {
                for (int i = 0; i < archive_data_view.SelectedRows.Count; i++)
                {
                    string selected_email = archive_data_view.SelectedRows[i].Cells["EMAILID"].Value.ToString();
                    string update_query_selected = "UPDATE [EMAIL] SET ARCHIVED = 0 WHERE EMAILID = " + selected_email;
                    SqlCommand validateCmd = new SqlCommand(update_query_selected, conn);
                    validateCmd.ExecuteNonQuery();
                    refreshInbox();
                }
            }
            conn.Close();
        }

        private void reply_button_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (inbox_data_view.SelectedRows.Count != 1) return;

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            string sender_id = inbox_data_view.SelectedRows[0].Cells["SENDERID"].Value.ToString();
            string sender_email_query = "SELECT [EMAIL] FROM [USER] WHERE USERID = " + sender_id;

            SqlCommand validateCmd = new SqlCommand(sender_email_query, conn);
            string sender_email = validateCmd.ExecuteScalar().ToString();

            conn.Close();

            send_new sendForm = new send_new(sender_email);
            sendForm.ShowDialog();
        }

        private void forward_button_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (inbox_data_view.SelectedRows.Count != 1) return;
            string sender_body = inbox_data_view.SelectedRows[0].Cells["BODY"].Value.ToString();
            send_new sendForm = new send_new("", sender_body);
            sendForm.ShowDialog();
        }

        private void linkLabel2_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (archive_data_view.SelectedRows.Count != 1) return;

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            string sender_id = archive_data_view.SelectedRows[0].Cells["SENDERID"].Value.ToString();
            string sender_email_query = "SELECT [EMAIL] FROM [USER] WHERE USERID = " + sender_id;

            SqlCommand validateCmd = new SqlCommand(sender_email_query, conn);
            string sender_email = validateCmd.ExecuteScalar().ToString();

            conn.Close();

            send_new sendForm = new send_new(sender_email);
            sendForm.ShowDialog();
        }

        private void linkLabel1_LinkClicked_2(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (archive_data_view.SelectedRows.Count != 1) return;
            string sender_body = archive_data_view.SelectedRows[0].Cells["BODY"].Value.ToString();
            send_new sendForm = new send_new("", sender_body);
            sendForm.ShowDialog();
        }
    }
}
