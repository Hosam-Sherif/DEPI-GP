// Mazaad.Domain/Enums/CompanyDocumentType.cs

namespace Mazaad.Domain.Enums
{
    public enum CompanyDocumentType
    {
        /// <summary>السجل التجاري</summary>
        CommercialRegister = 1,

        /// <summary>البطاقة الضريبية</summary>
        TaxCard = 2,

        /// <summary>عقد التأسيس</summary>
        ArticlesOfAssociation = 3,

        /// <summary>أي مستند تاني</summary>
        Other = 99,
    }
}