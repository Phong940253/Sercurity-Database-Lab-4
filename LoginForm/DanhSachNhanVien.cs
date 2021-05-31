using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
namespace LoginForm
{
    public partial class DanhSachNhanVien : DevExpress.XtraEditors.XtraForm
    {
        public DanhSachNhanVien()
        {
            InitializeComponent();
        }

        string connectionString;
        DataTable NhanVien = new DataTable();
        SqlDataAdapter adapter;
        SqlDataAdapter addNhanVien = new SqlDataAdapter();
        string insertCommand = null;
        SqlConnection sqlConnection;

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private string getStringConnect()
        {
            SqlConnectionStringBuilder strconnect = new SqlConnectionStringBuilder();
            strconnect["server"] = "DESKTOP-T3GOB10\\SQLEXPRESS";
            strconnect["database"] = "QLSV";
            strconnect["trusted_connection"] = "true";
            return strconnect.ToString();
        }

        private void DanhSachNhanVien_Load(object sender, EventArgs e)
        {
            connectionString = getStringConnect();
            sqlConnection = new SqlConnection(connectionString);
            adapter = new SqlDataAdapter("EXEC SP_SEL_ENCRYPT_NHANVIEN", sqlConnection);
            //SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(adapter);
            adapter.Fill(NhanVien);
            dgvNhanVien.DataSource = NhanVien;
            NhanVien = decryptColumnLuong(NhanVien);
            showNhanVien(0);
            btnXoa.Enabled = false;
            btnSua.Enabled = false;
            btnLuu.Enabled = false;
            btnKhong.Enabled = false;
        }

        private DataTable decryptColumnLuong(DataTable NhanVien)
        {
            foreach (DataRow dataRow in NhanVien.Rows)
            {
                byte[] enc = StringToByteArray(dataRow["LUONGCB"].ToString());
                byte[] dec = AES.Decrypt(enc);
                dataRow["LUONGCB"] = Encoding.UTF8.GetString(dec);
                
            }
            return NhanVien;
        }

        private void rowDataGridViewClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            btnSua.Enabled = true;
            btnXoa.Enabled = true;
            showNhanVien(e.RowIndex);
            
        }

        private void showNhanVien(int index)
        {
            DataGridViewRow row = dgvNhanVien.Rows[index];
            string maNV = row.Cells[0].Value.ToString();
            sqlConnection.Open();
            string queryNhanVien = $"select MANV, HOTEN, EMAIL, MATKHAU, TENDN, cast(LUONG as varchar(max)) as LUONGCB from nhanvien where manv = '{maNV}'";
            SqlCommand cmd = new SqlCommand(queryNhanVien, sqlConnection);
            var nhanVien = cmd.ExecuteReader();
            if (nhanVien.HasRows)
            {
                nhanVien.Read();
                textMaNV.Text = maNV;
                textHoTen.Text = nhanVien["HOTEN"].ToString();
                textEmail.Text = nhanVien["EMAIL"].ToString();
                textTenDN.Text = nhanVien["TENDN"].ToString();
                textMatKhau.Text = nhanVien["MATKHAU"].ToString();
                string luong = nhanVien["LUONGCB"].ToString();
                byte[] enc = StringToByteArray(luong);
                byte[] dec = AES.Decrypt(enc);
                textLuong.Value = Int64.Parse(Encoding.UTF8.GetString(dec));
            }
            sqlConnection.Close();
        }


        private void dgvNhanVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
        }

        private void DanhSachNhanVien_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Đồng ý hoặc không?",
                       "Thoát",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void textLuong_EditValueChanged(object sender, EventArgs e)
        {
            byte[] enc = AES.Encrypt(Encoding.UTF8.GetBytes(textLuong.Text));
            string cipherText = Encoding.UTF8.GetString(enc);
            string res = string.Join("", cipherText.Select(c => String.Format("{0:X2}", Convert.ToInt32(c))));
            byte[] dec = AES.Decrypt(enc);
            Console.WriteLine(res + " " + Encoding.UTF8.GetString(dec));
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public static byte[] StringToByteArray(String hex)
        {
            Console.WriteLine(hex);
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow dataRow = NhanVien.NewRow();
                dataRow["MANV"] = textMaNV.Text;
                dataRow["HOTEN"] = textHoTen.Text;
                dataRow["EMAIL"] = textEmail.Text;
                dataRow["LUONGCB"] = textLuong.Value;
                NhanVien.Rows.Add(dataRow);
                //addSinhVien.InsertCommand = new SqlCommand(insertCommand + ")", sqlConnection);
                byte[] enc = AES.Encrypt(Encoding.UTF8.GetBytes(textLuong.Text));
                string cipherText = ByteArrayToString(enc);
                string passwordHash = CommonEncrypt.Hash(textMatKhau.Text);
                Console.WriteLine(passwordHash);
                insertCommand += $"EXEC SP_INS_ENCRYPT_NHANVIEN '{textMaNV.Text}', N'{textHoTen.Text}', '{textEmail.Text}', '{cipherText}', '{textTenDN.Text}', '{passwordHash}';";
                Console.WriteLine(insertCommand);
                btnLuu.Enabled = true;
                btnKhong.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Thêm không thành công! Lỗi:  " + ex.Message);
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(insertCommand))
            {
                try
                {
                    sqlConnection.Open();
                    SqlCommand cmd = new SqlCommand(insertCommand, sqlConnection);
                    cmd.ExecuteNonQuery();
                    sqlConnection.Close();
                    insertCommand = "";
                    btnLuu.Enabled = false;
                    btnKhong.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Đồng ý hoặc không?",
                      "Thoát",
                       MessageBoxButtons.YesNo,
                       MessageBoxIcon.Information) == DialogResult.No)
            {
                return;
            }
            try
            {
                int rowSelected = dgvNhanVien.CurrentRow.Index;
                insertCommand += $"delete from nhanvien where manv = '{dgvNhanVien.Rows[rowSelected].Cells[0].Value.ToString()}'";
                dgvNhanVien.Rows.Remove(dgvNhanVien.Rows[rowSelected]);
                btnLuu.Enabled = true;
                btnKhong.Enabled = true;
            }
            catch
            {
                MessageBox.Show("Xóa không thành công!");
            }
        }

        private void btnKhong_Click(object sender, EventArgs e)
        {
            btnLuu.Enabled = false;
            insertCommand = "";
            NhanVien = new DataTable();
            adapter = new SqlDataAdapter("EXEC SP_SEL_ENCRYPT_NHANVIEN", sqlConnection);
            adapter.Fill(NhanVien);
            dgvNhanVien.DataSource = NhanVien;
            NhanVien = decryptColumnLuong(NhanVien);
        }

        private void DanhSachNhanVien_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}