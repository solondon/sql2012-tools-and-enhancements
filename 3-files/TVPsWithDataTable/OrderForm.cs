using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TVPsWithDataTable
{
  public partial class OrderForm : Form
  {
    public OrderForm()
    {
      InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      this.OrderBindingSource.AddNew();
      this.DateTimePicker1.Value = DateTime.Now;
    }

    private void Button1_Click(object sender, EventArgs e)
    {
      this.OrderBindingSource.EndEdit();

      using (SqlConnection conn = new SqlConnection("Data Source=.;Initial Catalog=MyDb;Integrated Security=True;"))
      {
        conn.Open();

        using (SqlCommand cmd = new SqlCommand("uspInsertNewOrder", conn))
        {
          cmd.CommandType = CommandType.StoredProcedure;

          SqlParameter headerParam = cmd.Parameters.AddWithValue("@OrderHeader", this.OrderDS1.Order);
          SqlParameter detailsParam = cmd.Parameters.AddWithValue("@OrderDetails", this.OrderDS1.OrderDetail);

          headerParam.SqlDbType = SqlDbType.Structured;
          detailsParam.SqlDbType = SqlDbType.Structured;

          cmd.ExecuteNonQuery();
        }

        conn.Close();
      }
    }
  }
}
