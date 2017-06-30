using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YYXB.Model
{
    public enum ORDER_STATE
    {
        //待付款 0
        //已付款 1
        //待服务 2
        //服务中 3
        //待评价 4 
        //已完成/已评价 5

        //已取消        6       -1
        //申请退款中    7       -2
        //退款中        8       -3
        //退款成功      9       -4
        //已拒单       10       -5
        //过期未接单   11       -6
        //退款失败     12       -7

        UNPAID = 0,
        PAID = 1,
        PENDING_SERVICE = 2,
        SERVICEING = 3,
        PENDING_EVALUATION = 4,
        FINISHED = 5,
        CANNEL = 6,
        APPLICATION_OF_REFUND = 7,
        REFUNDINT = 8,
        REFUND_SUCCESS = 9,
        REFUSED = 10,
        EXPIRE = 11,
        REFUND_FAIL = 12
    }

    public class OrderInfo
    {
        public long orderid { get; set; }

        public string orderno { get; set; }

        public long p_userid { get; set; }

        public long userid { get; set; }

        public string begin_time { get; set; }

        public string end_time { get; set; }

        public int o_type { get; set; }

        public int n_type { get; set; }

        public long acceptor { get; set; }

        public int state { get; set; }

        public int hospitalid { get; set; }

        public string hospital_name { get; set; }

        public int area { get; set; }

        public string addr { get; set; }

        public int pickup { get; set; }

        public float pickup_cost { get; set; }

        public float nursing_unit_price { get; set; }

        public int nursing_cnt { get; set; }

        public float cost { get; set; }
    }

    public class VideoOrderInfo
    {
        public long v_orderid { get; set; }

        public string v_orderno { get; set; }

        public string orderno { get; set; }

        public int time_len { get; set; }

        public float cost { get; set; }

        public long userid { get; set; }

        public int state { get; set; }
    }
}
