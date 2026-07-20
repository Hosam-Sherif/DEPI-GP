using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mazaad.Domain.Enums
{
    /// <summary>
    /// حالة طلب الشراء في المزاد المعكوس
    /// </summary>
    public enum ReverseAuctionStatus
    {
        /// <summary>الطلب مفتوح لتلقّي العروض</summary>
        Open = 0,

        /// <summary>انتهت مدة الطلب دون اختيار عرض</summary>
        Closed = 1,

        /// <summary>تم اختيار عرض وإنشاء أمر شراء</summary>
        Awarded = 2,

        /// <summary>تم إلغاء الطلب من قِبَل الشركة الطالبة</summary>
        Cancelled = 3
    }
}
