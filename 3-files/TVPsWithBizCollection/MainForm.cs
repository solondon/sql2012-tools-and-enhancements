using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Collections;

namespace TVPsWithBizCollection
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var headers = new OrderHeaderCollection();
			var details = new OrderDetailCollection();

			headers.Add(new OrderHeader() { OrderId = 6, CustomerId = 51, OrderedAt = Convert.ToDateTime("1/1/2008") });
			details.Add(new OrderDetail() { OrderId = 6, LineNumber = 1, ProductId = 12, Quantity = 2, Price = 15.95m });
			details.Add(new OrderDetail() { OrderId = 6, LineNumber = 2, ProductId = 57, Quantity = 1, Price = 59.99m });
			details.Add(new OrderDetail() { OrderId = 6, LineNumber = 3, ProductId = 36, Quantity = 10, Price = 8.50m });

			headers.Add(new OrderHeader() { OrderId = 7, CustomerId = 51, OrderedAt = Convert.ToDateTime("1/2/2008") });
			details.Add(new OrderDetail() { OrderId = 7, LineNumber = 1, ProductId = 23, Quantity = 2, Price = 79.50m });
			details.Add(new OrderDetail() { OrderId = 7, LineNumber = 2, ProductId = 78, Quantity = 1, Price = 3.25m });

			using (var conn = new SqlConnection("Data Source=.;Initial Catalog=MyDb;Integrated Security=True;"))
			{
				conn.Open();

				using (var cmd = new SqlCommand("uspInsertOrders", conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;

					var headersParam = cmd.Parameters.AddWithValue("@OrderHeaders", headers);
					var detailsParam = cmd.Parameters.AddWithValue("@OrderDetails", details);

					headersParam.SqlDbType = SqlDbType.Structured;
					detailsParam.SqlDbType = SqlDbType.Structured;

					cmd.ExecuteNonQuery();
				}

				conn.Close();
			}
		}

		public class OrderHeader
		{
			public int OrderId { get; set; }
			public int CustomerId { get; set; }
			public DateTime OrderedAt { get; set; }
		}

		public class OrderDetail
		{
			public int OrderId { get; set; }
			public int LineNumber { get; set; }
			public int ProductId { get; set; }
			public int Quantity { get; set; }
			public decimal Price { get; set; }
		}

		public class OrderHeaderCollection : List<OrderHeader>, IEnumerable<SqlDataRecord>
		{
			// Custom iterator method to support TVPs in SQL Server 2008
			IEnumerator<SqlDataRecord> IEnumerable<SqlDataRecord>.GetEnumerator()
			{
				var sdr = new SqlDataRecord(
					new SqlMetaData("OrderId", SqlDbType.Int),
					new SqlMetaData("CustomerId", SqlDbType.Int),
					new SqlMetaData("OrderedAt", SqlDbType.Date));

				foreach (OrderHeader oh in this)
				{
					sdr.SetInt32(0, oh.OrderId);
					sdr.SetInt32(1, oh.CustomerId);
					sdr.SetDateTime(2, oh.OrderedAt);

					yield return sdr;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return base.GetEnumerator();
			}
		}

		public class OrderDetailCollection : List<OrderDetail>, IEnumerable<SqlDataRecord>
		{
			// Custom iterator method to support TVPs in SQL Server 2008
			IEnumerator<SqlDataRecord> IEnumerable<SqlDataRecord>.GetEnumerator()
			{
				var sdr = new SqlDataRecord(
					new SqlMetaData("OrderId", SqlDbType.Int),
					new SqlMetaData("LineNumber", SqlDbType.Int),
					new SqlMetaData("ProductId", SqlDbType.Int),
					new SqlMetaData("Quantity", SqlDbType.Int),
					new SqlMetaData("Price", SqlDbType.Money));

				foreach (OrderDetail od in this)
				{
					sdr.SetInt32(0, od.OrderId);
					sdr.SetInt32(1, od.LineNumber);
					sdr.SetInt32(2, od.ProductId);
					sdr.SetInt32(3, od.Quantity);
					sdr.SetDecimal(4, od.Price);

					yield return sdr;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return base.GetEnumerator();
			}
		}
	}
}
