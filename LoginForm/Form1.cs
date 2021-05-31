using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace LoginForm
{


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void simpleButton11_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection("Data Source=DESKTOP-T3GOB10\\SQLEXPRESS;Initial Catalog=QLSV;Integrated Security=SSPI"))
            {
                bool success = false;
                try
                {
                    string username = textUser.Text;
                    string passwordMd5 = CreateMD5(textPassword.Text);
                    string passwordHash = Hash(textPassword.Text);

                    conn.Open();
                    SqlCommand cmd = new SqlCommand($"SELECT COUNT(*) FROM SINHVIEN where TENDN = '{username}' and cast(MATKHAU as varchar(max)) = '{passwordMd5}'", conn);
                    Console.WriteLine(cmd.CommandText);
                    int res = Convert.ToInt32(cmd.ExecuteScalar());
                    if (res > 0) success = true;


                    cmd = new SqlCommand($"SELECT COUNT(*) FROM NHANVIEN where TENDN = '{username}' and cast(MATKHAU as varchar(max)) = '{passwordHash}'", conn);
                    Console.WriteLine(cmd.CommandText);
                    res = Convert.ToInt32(cmd.ExecuteScalar());
                    if (res > 0) success = true;
                    
                    if (success){
                        MessageBox.Show("Đăng nhập thành công!");
                        this.Hide();
                        Form danhsachnhanvien = new DanhSachNhanVien();
                        danhsachnhanvien.ShowDialog();
                    } else
                    {
                        MessageBox.Show("tên đăng nhập và mật khẩu không hợp lệ");
                    }
                    

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ERROR: Lỗi database hoặc kết nối, {ex.Message}");
                }
                conn.Close();
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }


        private string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}
