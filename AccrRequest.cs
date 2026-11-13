using System.Text.Json.Serialization;
#nullable disable
namespace AccrServicesApi;
public class AccrRequest
{
	[JsonPropertyName("header")]
	public Header Header { get; set; }

	[JsonPropertyName("credits")]
	public List<Credit> Credits { get; set; }
}

public class Header
{
	[JsonPropertyName("bankName")]
	public string BankName { get; set; }

	[JsonPropertyName("bankID")]
	public string BankID { get; set; }

	[JsonPropertyName("reportingDate")]
	public string ReportingDate { get; set; }

	[JsonPropertyName("responsiblePerson")]
	public ResponsiblePerson ResponsiblePerson { get; set; }
}

public class ResponsiblePerson
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("jobtitle")]
	public string Jobtitle { get; set; }

	[JsonPropertyName("pinCode")]
	public string PinCode { get; set; }

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("dateOfBirth")]
	public string DateOfBirth { get; set; }
}

public class Borrower
{
	[JsonIgnore]
	public long CreditId { get; set; }
	[JsonPropertyName("cif")]
	public string Cif { get; set; }

	[JsonPropertyName("balanceAccountNo")]
	public string BalanceAccountNo { get; set; }

	[JsonPropertyName("relationWithBanks")]
	public List<RelationWithBank> RelationWithBanks { get; set; }

	[JsonPropertyName("group")]
	public Group Group { get; set; }

	[JsonPropertyName("groupForBank")]
	public GroupForBank GroupForBank { get; set; }

	[JsonPropertyName("isCoBorrower")]
	public bool? IsCoBorrower { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("id")]
	public string DocId { get; set; }

	[JsonPropertyName("dateOfBirth")]
	public string DateOfBirth { get; set; }

	[JsonPropertyName("placeOfBirth")]
	public string PlaceOfBirth { get; set; }

	[JsonPropertyName("countryCode")]
	public string CountryCode { get; set; }

	[JsonPropertyName("pinCode")]
	public string PinCode { get; set; }

	[JsonPropertyName("isBrwDiedOrDisappeared")]
	public bool? IsBrwDiedOrDisappeared { get; set; }

	[JsonPropertyName("bankruptcyStatus")]
	public bool? BankruptcyStatus { get; set; }

	[JsonPropertyName("income")]
	public Income Income { get; set; }

	[JsonPropertyName("workPlace")]
	public WorkPlace WorkPlace { get; set; }

	[JsonPropertyName("bankCardCode")]
	public string BankCardCode { get; set; }

	[JsonPropertyName("workAreaCode")]
	public string WorkAreaCode { get; set; }

	[JsonPropertyName("foreignEcoActivities")]
	public List<ForeignEcoActivity> ForeignEcoActivities { get; set; }

	[JsonPropertyName("generalManager")]
	public string GeneralManager { get; set; }

	[JsonPropertyName("shareholders")]
	public List<Shareholder> Shareholders { get; set; }

	[JsonPropertyName("businessCategory")]
	public string BusinessCategory { get; set; }

	[JsonPropertyName("isOpLost")]
	public bool? IsOpLost { get; set; }

	[JsonPropertyName("financeReports")]
	public List<FinanceReport> FinanceReports { get; set; }
}

public class RelationWithBank
{
	[JsonPropertyName("isRelatedBank")]
	public bool? IsRelatedBank { get; set; }

	[JsonPropertyName("relatedName")]
	public string RelatedName { get; set; }

	[JsonPropertyName("relatedPinCode")]
	public string RelatedPinCode { get; set; }

	[JsonPropertyName("relatedId")]
	public string RelatedId { get; set; }

	[JsonPropertyName("relatedDateOfBirth")]
	public string RelatedDateOfBirth { get; set; }

	[JsonPropertyName("relationCode")]
	public string RelationCode { get; set; }
}

public class Group
{
	[JsonPropertyName("code")]
	public string Code { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }
}

public class GroupForBank
{
	[JsonPropertyName("code")]
	public string Code { get; set; }

	[JsonPropertyName("insolvencyStatus")]
	public string InsolvencyStatus { get; set; }
}

public class Income
{
	[JsonPropertyName("netIncome")]
	public decimal NetIncome { get; set; } // int-dən decimal-a dəyişdirildi

	[JsonPropertyName("incomeForeignCurrency")]
	public IncomeForeignCurrency IncomeForeignCurrency { get; set; }

	[JsonPropertyName("bgnCoefficient")]
	public decimal BgnCoefficient { get; set; } // int-dən decimal-a dəyişdirildi

	[JsonPropertyName("bgnOverdue")]
	public int BgnOverdue { get; set; } // Gecikmə günləridir, int olaraq qalır
}

public class IncomeForeignCurrency
{
	[JsonPropertyName("netIncomeForeignCurrency")]
	public decimal NetIncomeForeignCurrency { get; set; } // int-dən decimal-a dəyişdirildi

	[JsonPropertyName("currencyCode")]
	public string CurrencyCode { get; set; }
}

public class WorkPlace
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("tin")]
	public string Tin { get; set; }

	[JsonPropertyName("workStatus")]
	public string WorkStatus { get; set; }
}

public class ForeignEcoActivity
{
	[JsonPropertyName("isForeignEcoOp")]
	public bool? IsForeignEcoOp { get; set; }

	[JsonPropertyName("importExportCode")]
	public string ImportExportCode { get; set; }
}

public class Collateral
{
	[JsonIgnore]
	public long CreditId { get; set; }
	[JsonPropertyName("collateralTypeCode")]
	public string CollateralTypeCode { get; set; }

	[JsonPropertyName("anyInfoToDisting")]
	public string AnyInfoToDisting { get; set; }

	[JsonPropertyName("collateralValue")]
	public decimal CollateralValue { get; set; }

	[JsonPropertyName("marketValue")]
	public decimal MarketValue { get; set; }

	[JsonPropertyName("registryAgency")]
	public string RegistryAgency { get; set; }

	[JsonPropertyName("registryDate")]
	public string RegistryDate { get; set; }

	[JsonPropertyName("registryNo")]
	public string RegistryNo { get; set; }

	[JsonPropertyName("evaluation")]
	public Evaluation Evaluation { get; set; }

	[JsonPropertyName("insurance")]
	public Insurance Insurance { get; set; }

	[JsonPropertyName("collateralSubTypeCode")]
	public string CollateralSubTypeCode { get; set; }

	[JsonPropertyName("collateralGroupCode")]
	public string CollateralGroupCode { get; set; }
}

public class Credit
{
	[JsonIgnore]
	public long Id { get; set; }
	[JsonPropertyName("borrower")]
	public Borrower Borrower { get; set; }

	[JsonPropertyName("accountNo")]
	public string AccountNo { get; set; }

	[JsonPropertyName("creditType")]
	public string CreditType { get; set; }

	[JsonPropertyName("purposeOfCredit")]
	public string PurposeOfCredit { get; set; }

	[JsonPropertyName("economicArea")]
	public string EconomicArea { get; set; }

	[JsonPropertyName("agrocode")]
	public string Agrocode { get; set; }

	[JsonPropertyName("creditStatusCode")]
	public string CreditStatusCode { get; set; }

	[JsonPropertyName("ktnCoefficient")]
	public decimal KtnCoefficient { get; set; } // int-dən decimal-a

	[JsonPropertyName("initialAmountOfCredit")]
	public decimal InitialAmountOfCredit { get; set; } // int-dən decimal-a

	[JsonPropertyName("creditLineAmount")]
	public decimal CreditLineAmount { get; set; } // int-dən decimal-a

	[JsonPropertyName("fullAmountCreditLine")]
	public decimal FullAmountCreditLine { get; set; } // int-dən decimal-a

	[JsonPropertyName("uncommittedAmountCreditLine")]
	public decimal UncommittedAmountCreditLine { get; set; } // int-dən decimal-a

	[JsonPropertyName("usedAmountCreditLine")]
	public decimal UsedAmountCreditLine { get; set; } // int-dən decimal-a

	[JsonPropertyName("uncommittedUsedAmountCreditLine")]
	public decimal UncommittedUsedAmountCreditLine { get; set; } // int-dən decimal-a

	[JsonPropertyName("disoutAmountOfCredit")]
	public decimal DisoutAmountOfCredit { get; set; } // int-dən decimal-a

	[JsonPropertyName("oiaForRepperiod")]
	public decimal OiaForRepperiod { get; set; }

	[JsonPropertyName("oiaForNotOverdueInterest")]
	public decimal OiaForNotOverdueInterest { get; set; }

	[JsonPropertyName("oiaForOverdueInterest")]
	public decimal OiaForOverdueInterest { get; set; }

	[JsonPropertyName("penaltyDebt")]
	public decimal PenaltyDebt { get; set; }

	[JsonPropertyName("paidMainAmount")]
	public decimal PaidMainAmount { get; set; } // int-dən decimal-a

	[JsonPropertyName("paidInterestAmount")]
	public decimal PaidInterestAmount { get; set; } // int-dən decimal-a

	[JsonPropertyName("creditRegion")]
	public string CreditRegion { get; set; }

	[JsonPropertyName("currencyOfCredit")]
	public string CurrencyOfCredit { get; set; }

	[JsonPropertyName("dateOfGrant")]
	public string DateOfGrant { get; set; }

	[JsonPropertyName("dueTimeFirstContract")]
	public string DueTimeFirstContract { get; set; }

	[JsonPropertyName("dueTimeLastContract")]
	public string DueTimeLastContract { get; set; }

	[JsonPropertyName("creditPeriodInMonths")]
	public int CreditPeriodInMonths { get; set; } // Ay sayıdır, int qalır

	[JsonPropertyName("monthlyPaymentAmount")]
	public decimal MonthlyPaymentAmount { get; set; }

	[JsonPropertyName("annualInterestRate")]
	public decimal AnnualInterestRate { get; set; }

	[JsonPropertyName("fifd")]
	public decimal Fifd { get; set; }

	[JsonPropertyName("interestRateTypeCode")]
	public string InterestRateTypeCode { get; set; }

	[JsonPropertyName("lastPaymentDate")]
	public string LastPaymentDate { get; set; }

	[JsonPropertyName("amountFromCollateral")]
	public decimal AmountFromCollateral { get; set; } // int-dən decimal-a

	[JsonPropertyName("forgivenAmount")]
	public decimal ForgivenAmount { get; set; } // int-dən decimal-a

	[JsonPropertyName("paidAmountFromCollateralSells")]
	public decimal PaidAmountFromCollateralSells { get; set; } // int-dən decimal-a

	[JsonPropertyName("borrowersOfBalanceAmount")]
	public decimal BorrowersOfBalanceAmount { get; set; } // int-dən decimal-a

	[JsonPropertyName("overdueInstallmentOnPrincipal")]
	public decimal OverdueInstallmentOnPrincipal { get; set; } // int-dən decimal-a

	[JsonPropertyName("generalPurposeCode")]
	public string GeneralPurposeCode { get; set; }

	[JsonPropertyName("fundingSourceCode")]
	public string FundingSourceCode { get; set; }

	[JsonPropertyName("isSoldCollateral")]
	public bool? IsSoldCollateral { get; set; }

	[JsonPropertyName("isEvaluationDoc")]
	public bool? IsEvaluationDoc { get; set; }

	[JsonPropertyName("branchCode")]
	public string BranchCode { get; set; }

	[JsonPropertyName("branchName")]
	public string BranchName { get; set; }

	[JsonPropertyName("judicialStatusCode")]
	public string JudicialStatusCode { get; set; }

	[JsonPropertyName("mainPaymentPeriodDays")]
	public int MainPaymentPeriodDays { get; set; } // Gün sayıdır, int qalır

	[JsonPropertyName("interestPaymentPeriodDays")]
	public int InterestPaymentPeriodDays { get; set; } // Gün sayıdır, int qalır

	[JsonPropertyName("lastPaidMainAmount")]
	public decimal LastPaidMainAmount { get; set; } // int-dən decimal-a

	[JsonPropertyName("lastPaidInterestAmount")]
	public decimal LastPaidInterestAmount { get; set; } // int-dən decimal-a

	[JsonPropertyName("gracePeriodByPrincipalAmount")]
	public int GracePeriodByPrincipalAmount { get; set; } // Gün sayıdır, int qalır

	[JsonPropertyName("isBehavioralModelUsed")]
	public bool? IsBehavioralModelUsed { get; set; }

	[JsonPropertyName("insurance")]
	public Insurance Insurance { get; set; }

	[JsonPropertyName("collaterals")]
	public List<Collateral> Collaterals { get; set; }

	[JsonPropertyName("guarantees")]
	public List<Guarantee> Guarantees { get; set; }

	[JsonPropertyName("govWarrantyCode")]
	public string GovWarrantyCode { get; set; }

	[JsonPropertyName("subsidyByGov")]
	public string SubsidyByGov { get; set; }

	[JsonPropertyName("isDocumentsPresented")]
	public bool? IsDocumentsPresented { get; set; }

	[JsonPropertyName("isProjectFinanced")]
	public bool? IsProjectFinanced { get; set; }

	[JsonPropertyName("isBorrowerHedged")]
	public bool? IsBorrowerHedged { get; set; }

	[JsonPropertyName("riskVolume")]
	public string RiskVolume { get; set; } // int-dən decimal-a

	[JsonPropertyName("riskWeightedAssetsAmount")]
	public decimal RiskWeightedAssetsAmount { get; set; } // int-dən decimal-a

	[JsonPropertyName("refinancedReasonCode")]
	public string RefinancedReasonCode { get; set; }

	[JsonPropertyName("restructuring")]
	public Restructuring Restructuring { get; set; }

	[JsonPropertyName("isDelayedProject")]
	public bool? IsDelayedProject { get; set; }

	[JsonPropertyName("compareNonStandActTotalLiab")]
	public bool? CompareNonStandActTotalLiab { get; set; }

	[JsonPropertyName("isEffecable")]
	public bool? IsEffecable { get; set; }

	[JsonPropertyName("provisionInfo")]
	public ProvisionInfo ProvisionInfo { get; set; }

	[JsonPropertyName("ifrsClassificationCode")]
	public string IfrsClassificationCode { get; set; }

	[JsonPropertyName("reserveAmountBasedOnIFRS9")]
	public decimal ReserveAmountBasedOnIFRS9 { get; set; } // int-dən decimal-a

	[JsonPropertyName("dscr")]
	public decimal Dscr { get; set; } // int-dən decimal-a (əmsaldır)

	[JsonPropertyName("creditClassCode")]
	public string CreditClassCode { get; set; }

	[JsonPropertyName("daysMainSumIsOverdue")]
	public int DaysMainSumIsOverdue { get; set; }

	[JsonPropertyName("daysInterestIsOverdue")]
	public int DaysInterestIsOverdue { get; set; }

	[JsonPropertyName("ratingOfBorrower")]
	public RatingOfBorrower RatingOfBorrower { get; set; }

	[JsonPropertyName("businessEntityLegalStatus")]
	public string BusinessEntityLegalStatus { get; set; }

	[JsonPropertyName("portfolioType")]
	public string PortfolioType { get; set; }
}

public class Evaluation
{
	[JsonPropertyName("evaluationDateByBank")]
	public string EvaluationDateByBank { get; set; }

	[JsonPropertyName("evaluationDateByExtevaluator")]
	public string EvaluationDateByExtevaluator { get; set; }

	[JsonPropertyName("externalEvaluatorName")]
	public string ExternalEvaluatorName { get; set; }
}

public class FinanceReport
{
	[JsonPropertyName("reportingDate")]
	public string ReportingDate { get; set; }

	[JsonPropertyName("reportType")]
	public string ReportType { get; set; }

	[JsonPropertyName("revenue")]
	public decimal Revenue { get; set; } // Hamısı int-dən decimal-a keçirildi

	[JsonPropertyName("ebitda")]
	public decimal Ebitda { get; set; }

	[JsonPropertyName("ebit")]
	public decimal Ebit { get; set; }

	[JsonPropertyName("interestExpences")]
	public decimal InterestExpences { get; set; }

	[JsonPropertyName("netProfit")]
	public decimal NetProfit { get; set; }

	[JsonPropertyName("totalAssets")]
	public decimal TotalAssets { get; set; }

	[JsonPropertyName("totalAssetsForeignCurrency")]
	public decimal TotalAssetsForeignCurrency { get; set; }

	[JsonPropertyName("totalLiabilitiesForeignCurrency")]
	public decimal TotalLiabilitiesForeignCurrency { get; set; }

	[JsonPropertyName("interestExpenceLiabilities")]
	public decimal InterestExpenceLiabilities { get; set; }

	[JsonPropertyName("totalLiabilities")]
	public decimal TotalLiabilities { get; set; }

	[JsonPropertyName("foreignInterestExpenceLiabilities")]
	public decimal ForeignInterestExpenceLiabilities { get; set; }

	[JsonPropertyName("totalCapital")]
	public decimal TotalCapital { get; set; }

	[JsonPropertyName("insolvencyStatus")]
	public bool InsolvencyStatus { get; set; }
}

public class Guarantee
{
	[JsonIgnore]
	public long CreditId { get; set; }
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("id")]
	public string DocId { get; set; }

	[JsonPropertyName("pinCode")]
	public string PinCode { get; set; }

	[JsonPropertyName("countryCode")]
	public string CountryCode { get; set; }

	[JsonPropertyName("dateOfBirth")]
	public string DateOfBirth { get; set; }

	[JsonPropertyName("placeOfBirth")]
	public string PlaceOfBirth { get; set; }
}

public class Insurance
{
	[JsonPropertyName("insuranceCompName")]
	public string InsuranceCompName { get; set; }

	[JsonPropertyName("insuranceCompTIN")]
	public string InsuranceCompTIN { get; set; }

	[JsonPropertyName("insuranceEndDate")]
	public string InsuranceEndDate { get; set; }

	[JsonPropertyName("isBorrowerInsurance")]
	public bool? IsBorrowerInsurance { get; set; } // Nullable, çünki ancaq Borcalanda tələb olunur

	[JsonPropertyName("collateralInsurance")]
	public bool? CollateralInsurance { get; set; } // Nullable, çünki ancaq Girovda tələb olunur
}

public class ProvisionInfo
{
	// Səhifə 42-yə əsasən bu sahələr 0-100% arası dərəcə və ya məbləğdir, hamısı rəqəmsaldır.
	[JsonPropertyName("mainAmountProvisionDegree")]
	public decimal? MainAmountProvisionDegree { get; set; }

	[JsonPropertyName("mainAmountProvisionAmount")]
	public decimal MainAmountProvisionAmount { get; set; }

	[JsonPropertyName("interestAmountProvisionDegree")]
	public decimal InterestAmountProvisionDegree { get; set; }

	[JsonPropertyName("interestAmountProvisionAmount")]
	public decimal InterestAmountProvisionAmount { get; set; }
}

public class RatingOfBorrower
{
	[JsonPropertyName("ratingByBank")]
	public string RatingByBank { get; set; }

	[JsonPropertyName("ratingByOrg")]
	public string RatingByOrg { get; set; }

	[JsonPropertyName("ratingOrgName")]
	public string RatingOrgName { get; set; }
}

public class Restructuring
{
	[JsonPropertyName("restructuringCount")]
	public int RestructuringCount { get; set; }

	[JsonPropertyName("restructuringCount30072022")]
	public int RestructuringCount30072022 { get; set; }

	// DİQQƏT: Sənədin 25-ci səhifəsində açıqlamada "Debt" yazılsa da JSON strukturunda "rebt" yazılıb. 
	// Həmçinin açıqlaması məbləğ (amount) olduğu üçün tipi decimal olmalıdır.
	[JsonPropertyName("debtLastRestructDate")]
	public decimal DebtLastRestructDate { get; set; }

	[JsonPropertyName("oiaRepLastRestructDate")]
	public decimal OiaRepLastRestructDate { get; set; }

	[JsonPropertyName("classCodeLastRestructDate")]
	public string ClassCodeLastRestructDate { get; set; }

	[JsonPropertyName("overdueLastRestructDate")]
	public int OverdueLastRestructDate { get; set; } // Gecikmə günüdür, int olaraq qalır

	[JsonPropertyName("sumOverdueLastTwelveMonths")]
	public int SumOverdueLastTwelveMonths { get; set; } // Gecikmə günlərinin cəmidir, int olaraq qalır
}

public class Shareholder
{
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("pinCode")]
	public string PinCode { get; set; }

	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("dateOfBirth")]
	public string DateOfBirth { get; set; }
}