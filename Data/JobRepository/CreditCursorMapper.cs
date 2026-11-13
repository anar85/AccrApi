using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Globalization;

namespace AccrServicesApi.Data.JobRepository
{
    internal static class CreditCursorMapper
    {
        public static async Task<(List<Credit> Credits, List<Borrower> Borrowers, List<Collateral> Collaterals, List<Guarantee> Guarantees)> ReadAsync(OracleCommand cmd)
        {
            var credits = new List<Credit>();
            var borrowers = new List<Borrower>();
            var collaterals = new List<Collateral>();
            var guarantees = new List<Guarantee>();
            var creditDict = new Dictionary<long, Credit>();
            using (var reader = ((Oracle.ManagedDataAccess.Types.OracleRefCursor)cmd.Parameters["P_CREDITS_CUR"].Value).GetDataReader())
            {
                while (await reader.ReadAsync())
                {
                    var credit = new Credit
                    {
                        Id = Convert.ToInt64(reader["ID"]),
                        AccountNo = reader["ACCOUNT_NO"]?.ToString(),
                        CreditType = reader["CREDIT_TYPE"]?.ToString(),
                        PurposeOfCredit = reader["PURPOSE_OF_CREDIT"] == DBNull.Value ? null : reader["PURPOSE_OF_CREDIT"]?.ToString(),
                        EconomicArea = reader["ECONOMIC_AREA"] == DBNull.Value ? null : reader["ECONOMIC_AREA"]?.ToString(),
                        Agrocode = reader["AGROCODE"] == DBNull.Value ? null : reader["AGROCODE"]?.ToString(),
                        CreditStatusCode = reader["CREDIT_STATUS_CODE"] == DBNull.Value ? null : reader["CREDIT_STATUS_CODE"]?.ToString(),
                        CreditRegion = reader["CREDIT_REGION"] == DBNull.Value ? null : reader["CREDIT_REGION"]?.ToString(),
                        CurrencyOfCredit = reader["CURRENCY_OF_CREDIT"] == DBNull.Value ? null : reader["CURRENCY_OF_CREDIT"]?.ToString(),
                        InterestRateTypeCode = reader["INT_RATE_TYPE_CODE"] == DBNull.Value ? null : reader["INT_RATE_TYPE_CODE"]?.ToString(),
                        GeneralPurposeCode = reader["GENERAL_PURPOSE_CODE"] == DBNull.Value ? null : reader["GENERAL_PURPOSE_CODE"]?.ToString(),
                        FundingSourceCode = reader["FUNDING_SOURCE_CODE"] == DBNull.Value ? null : reader["FUNDING_SOURCE_CODE"]?.ToString(),
                        BranchCode = reader["BRANCH_CODE"] == DBNull.Value ? null : reader["BRANCH_CODE"]?.ToString(),
                        BranchName = reader["BRANCH_NAME"] == DBNull.Value ? null : reader["BRANCH_NAME"]?.ToString(),
                        JudicialStatusCode = reader["JUDICIAL_STATUS_CODE"] == DBNull.Value ? null : reader["JUDICIAL_STATUS_CODE"]?.ToString(),
                        GovWarrantyCode = reader["GOV_WARRANTY_CODE"] == DBNull.Value ? null : reader["GOV_WARRANTY_CODE"]?.ToString(),
                        SubsidyByGov = reader["SUBSIDY_BY_GOV"] == DBNull.Value ? null : reader["SUBSIDY_BY_GOV"]?.ToString(),
                        RefinancedReasonCode = reader["REFINANCED_REASON_CODE"] == DBNull.Value ? null : reader["REFINANCED_REASON_CODE"]?.ToString(),
                        IfrsClassificationCode = reader["IFRS_CLASSIFICATION_CODE"] == DBNull.Value ? null : reader["IFRS_CLASSIFICATION_CODE"]?.ToString(),
                        CreditClassCode = reader["CREDIT_CLASS_CODE"] == DBNull.Value ? null : reader["CREDIT_CLASS_CODE"]?.ToString(),
                        BusinessEntityLegalStatus = reader["BUSINESS_ENTITY_LEGAL_STATUS"] == DBNull.Value ? null : reader["BUSINESS_ENTITY_LEGAL_STATUS"]?.ToString(),
                        PortfolioType = reader["PORTFOLIO_TYPE"] == DBNull.Value ? null : reader["PORTFOLIO_TYPE"]?.ToString(),
                        RiskVolume = reader["RISK_VOLUME"] == DBNull.Value ? null : reader["RISK_VOLUME"]?.ToString(),

                        // Tarix tipl?ri (dd/MM/yyyy)
                        DateOfGrant = reader["DATE_OF_GRANT"] != DBNull.Value ? Convert.ToDateTime(reader["DATE_OF_GRANT"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                        DueTimeFirstContract = reader["DUE_TIME_FIRST_CONTRACT"] != DBNull.Value ? Convert.ToDateTime(reader["DUE_TIME_FIRST_CONTRACT"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                        DueTimeLastContract = reader["DUE_TIME_LAST_CONTRACT"] != DBNull.Value ? Convert.ToDateTime(reader["DUE_TIME_LAST_CONTRACT"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                        LastPaymentDate = reader["LAST_PAYMENT_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["LAST_PAYMENT_DATE"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,

                        // Decimal (M?bl?g v? Faiz) tipl?ri
                        KtnCoefficient = reader["KTN_COEFFICIENT"] != DBNull.Value ? Convert.ToDecimal(reader["KTN_COEFFICIENT"]) : 0,
                        InitialAmountOfCredit = reader["INITIAL_AMOUNT_OF_CREDIT"] != DBNull.Value ? Convert.ToDecimal(reader["INITIAL_AMOUNT_OF_CREDIT"]) : 0,
                        CreditLineAmount = reader["CREDIT_LINE_AMOUNT"] != DBNull.Value ? Convert.ToDecimal(reader["CREDIT_LINE_AMOUNT"]) : 0,
                        FullAmountCreditLine = reader["FULL_AMOUNT_CREDIT_LINE"] != DBNull.Value ? Convert.ToDecimal(reader["FULL_AMOUNT_CREDIT_LINE"]) : 0,
                        UncommittedAmountCreditLine = reader["UNCOMMITTED_AMT_CREDIT_LINE"] != DBNull.Value ? Convert.ToDecimal(reader["UNCOMMITTED_AMT_CREDIT_LINE"]) : 0,
                        UsedAmountCreditLine = reader["USED_AMOUNT_CREDIT_LINE"] != DBNull.Value ? Convert.ToDecimal(reader["USED_AMOUNT_CREDIT_LINE"]) : 0,
                        UncommittedUsedAmountCreditLine = reader["UNCOM_USED_AMT_CREDIT_LINE"] != DBNull.Value ? Convert.ToDecimal(reader["UNCOM_USED_AMT_CREDIT_LINE"]) : 0,
                        DisoutAmountOfCredit = reader["DISOUT_AMOUNT_OF_CREDIT"] != DBNull.Value ? Convert.ToDecimal(reader["DISOUT_AMOUNT_OF_CREDIT"]) : 0,
                        OiaForRepperiod = reader["OIA_FOR_REPPERIOD"] != DBNull.Value ? Convert.ToDecimal(reader["OIA_FOR_REPPERIOD"]) : 0,
                        OiaForNotOverdueInterest = reader["OIA_FOR_NOT_OVERDUE_INT"] != DBNull.Value ? Convert.ToDecimal(reader["OIA_FOR_NOT_OVERDUE_INT"]) : 0,
                        OiaForOverdueInterest = reader["OIA_FOR_OVERDUE_INT"] != DBNull.Value ? Convert.ToDecimal(reader["OIA_FOR_OVERDUE_INT"]) : 0,
                        PenaltyDebt = reader["PENALTY_DEBT"] != DBNull.Value ? Convert.ToDecimal(reader["PENALTY_DEBT"]) : 0,
                        PaidMainAmount = reader["PAID_MAIN_AMOUNT"] != DBNull.Value ? Convert.ToDecimal(reader["PAID_MAIN_AMOUNT"]) : 0,
                        PaidInterestAmount = reader["PAID_INTEREST_AMOUNT"] != DBNull.Value ? Convert.ToDecimal(reader["PAID_INTEREST_AMOUNT"]) : 0,
                        MonthlyPaymentAmount = reader["MONTHLY_PAYMENT_AMOUNT"] != DBNull.Value ? Convert.ToDecimal(reader["MONTHLY_PAYMENT_AMOUNT"]) : 0,
                        AnnualInterestRate = reader["ANNUAL_INTEREST_RATE"] != DBNull.Value ? Convert.ToDecimal(reader["ANNUAL_INTEREST_RATE"]) : 0,
                        Fifd = reader["FIFD"] != DBNull.Value ? Convert.ToDecimal(reader["FIFD"]) : 0,
                        AmountFromCollateral = reader["AMOUNT_FROM_COLLATERAL"] != DBNull.Value ? Convert.ToDecimal(reader["AMOUNT_FROM_COLLATERAL"]) : 0,
                        ForgivenAmount = reader["FORGIVEN_AMOUNT"] != DBNull.Value ? Convert.ToDecimal(reader["FORGIVEN_AMOUNT"]) : 0,
                        PaidAmountFromCollateralSells = reader["PAID_AMT_FROM_COL_SELLS"] != DBNull.Value ? Convert.ToDecimal(reader["PAID_AMT_FROM_COL_SELLS"]) : 0,
                        BorrowersOfBalanceAmount = reader["BORROWERS_OF_BALANCE_AMOUNT"] != DBNull.Value ? Convert.ToDecimal(reader["BORROWERS_OF_BALANCE_AMOUNT"]) : 0,
                        OverdueInstallmentOnPrincipal = reader["OVERDUE_INSTALL_ON_PRINCIPAL"] != DBNull.Value ? Convert.ToDecimal(reader["OVERDUE_INSTALL_ON_PRINCIPAL"]) : 0,
                        LastPaidMainAmount = reader["LAST_PAID_MAIN_AMOUNT"] != DBNull.Value ? Convert.ToDecimal(reader["LAST_PAID_MAIN_AMOUNT"]) : 0,
                        LastPaidInterestAmount = reader["LAST_PAID_INTEREST_AMOUNT"] != DBNull.Value ? Convert.ToDecimal(reader["LAST_PAID_INTEREST_AMOUNT"]) : 0,
                        RiskWeightedAssetsAmount = reader["RISK_WEIGHTED_ASSETS_AMOUNT"] != DBNull.Value ? Convert.ToDecimal(reader["RISK_WEIGHTED_ASSETS_AMOUNT"]) : 0,
                        ReserveAmountBasedOnIFRS9 = reader["RESERVE_AMOUNT_BASED_IFRS9"] != DBNull.Value ? Convert.ToDecimal(reader["RESERVE_AMOUNT_BASED_IFRS9"]) : 0,
                        Dscr = reader["DSCR"] != DBNull.Value ? Convert.ToDecimal(reader["DSCR"]) : 0,

                        // Int (Müdd?t v? Gecikm? günl?ri) tipl?ri
                        CreditPeriodInMonths = reader["CREDIT_PERIOD_IN_MONTHS"] != DBNull.Value ? Convert.ToInt32(reader["CREDIT_PERIOD_IN_MONTHS"]) : 0,
                        MainPaymentPeriodDays = reader["MAIN_PAYMENT_PERIOD_DAYS"] != DBNull.Value ? Convert.ToInt32(reader["MAIN_PAYMENT_PERIOD_DAYS"]) : 0,
                        InterestPaymentPeriodDays = reader["INT_PAYMENT_PERIOD_DAYS"] != DBNull.Value ? Convert.ToInt32(reader["INT_PAYMENT_PERIOD_DAYS"]) : 0,
                        GracePeriodByPrincipalAmount = reader["GRACE_PERIOD_BY_PRINCIPAL"] != DBNull.Value ? Convert.ToInt32(reader["GRACE_PERIOD_BY_PRINCIPAL"]) : 0,
                        DaysMainSumIsOverdue = reader["DAYS_MAIN_SUM_IS_OVERDUE"] != DBNull.Value ? Convert.ToInt32(reader["DAYS_MAIN_SUM_IS_OVERDUE"]) : 0,
                        DaysInterestIsOverdue = reader["DAYS_INTEREST_IS_OVERDUE"] != DBNull.Value ? Convert.ToInt32(reader["DAYS_INTEREST_IS_OVERDUE"]) : 0,

                        // Boolean tipl?ri
                        IsSoldCollateral = reader["IS_SOLD_COLLATERAL"] != DBNull.Value && Convert.ToInt32(reader["IS_SOLD_COLLATERAL"]) == 1 ? true : false,
                        IsEvaluationDoc = reader["IS_EVALUATION_DOC"] != DBNull.Value && Convert.ToInt32(reader["IS_EVALUATION_DOC"]) == 1,
                        IsBehavioralModelUsed = reader["IS_BEHAVIORAL_MODEL_USED"] != DBNull.Value && Convert.ToInt32(reader["IS_BEHAVIORAL_MODEL_USED"]) == 1,
                        IsDocumentsPresented = reader["IS_DOCUMENTS_PRESENTED"] != DBNull.Value && Convert.ToInt32(reader["IS_DOCUMENTS_PRESENTED"]) == 1,
                        IsProjectFinanced = reader["IS_PROJECT_FINANCED"] != DBNull.Value && Convert.ToInt32(reader["IS_PROJECT_FINANCED"]) == 1,
                        IsBorrowerHedged = reader["IS_BORROWER_HEDGED"] != DBNull.Value && Convert.ToInt32(reader["IS_BORROWER_HEDGED"]) == 1,
                        IsDelayedProject = reader["IS_DELAYED_PROJECT"] != DBNull.Value && Convert.ToInt32(reader["IS_DELAYED_PROJECT"]) == 1,
                        CompareNonStandActTotalLiab = reader["COMPARE_NONSTAND_ACT_TOT_LIAB"] != DBNull.Value && Convert.ToInt32(reader["COMPARE_NONSTAND_ACT_TOT_LIAB"]) == 1,
                        IsEffecable = reader["IS_EFFECABLE"] != DBNull.Value && Convert.ToInt32(reader["IS_EFFECABLE"]) == 1,

                        Insurance = new Insurance
                        {
                            InsuranceCompName = reader["INS_COMP_NAME"] != DBNull.Value ? reader["INS_COMP_NAME"].ToString() : null,
                            InsuranceCompTIN = reader["INS_COMP_TIN"] != DBNull.Value ? reader["INS_COMP_TIN"].ToString() : null,
                            InsuranceEndDate = reader["INS_END_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["INS_END_DATE"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                            IsBorrowerInsurance = reader["INS_IS_BORROWER_INS"] != DBNull.Value && Convert.ToInt32(reader["INS_IS_BORROWER_INS"]) == 1
                        },

                        ProvisionInfo = reader["PROV_MAIN_AMT"] != DBNull.Value ? new ProvisionInfo
                        {
                            MainAmountProvisionDegree = reader["PROV_MAIN_AMT_DEGREE"] != DBNull.Value ? Convert.ToDecimal(reader["PROV_MAIN_AMT_DEGREE"]) : null,
                            MainAmountProvisionAmount = reader["PROV_MAIN_AMT"] != DBNull.Value ? Convert.ToDecimal(reader["PROV_MAIN_AMT"]) : 0,
                            InterestAmountProvisionDegree = reader["PROV_INT_AMT_DEGREE"] != DBNull.Value ? Convert.ToDecimal(reader["PROV_INT_AMT_DEGREE"]) : 0,
                            InterestAmountProvisionAmount = reader["PROV_INT_AMT"] != DBNull.Value ? Convert.ToDecimal(reader["PROV_INT_AMT"]) : 0
                        } : null,

                        Restructuring = reader["RESTR_COUNT"] != DBNull.Value && Convert.ToInt32(reader["RESTR_COUNT"]) > 0 ? new Restructuring
                        {
                            RestructuringCount = Convert.ToInt32(reader["RESTR_COUNT"]),
                            RestructuringCount30072022 = reader["RESTR_COUNT_22072022"] != DBNull.Value ? Convert.ToInt32(reader["RESTR_COUNT_22072022"]) : 0,
                            DebtLastRestructDate = reader["RESTR_DEBT_LAST_DATE"] != DBNull.Value ? Convert.ToDecimal(reader["RESTR_DEBT_LAST_DATE"]) : 0,
                            OiaRepLastRestructDate = reader["RESTR_OIA_REP_LAST_DATE"] != DBNull.Value ? Convert.ToDecimal(reader["RESTR_OIA_REP_LAST_DATE"]) : 0,
                            ClassCodeLastRestructDate = reader["RESTR_RLASS_CODE_LAST_DATE"] == DBNull.Value ? null : reader["RESTR_RLASS_CODE_LAST_DATE"]?.ToString(),
                            OverdueLastRestructDate = reader["RESTR_OVERDUE_LAST_DATE"] != DBNull.Value ? Convert.ToInt32(reader["RESTR_OVERDUE_LAST_DATE"]) : 0,
                            SumOverdueLastTwelveMonths = reader["RESTR_SUM_OVERDUE_LAST_12M"] != DBNull.Value ? Convert.ToInt32(reader["RESTR_SUM_OVERDUE_LAST_12M"]) : 0
                        } : null,

                        RatingOfBorrower = reader["RATING_BY_BANK"] != DBNull.Value ? new RatingOfBorrower
                        {
                            RatingByBank = reader["RATING_BY_BANK"]?.ToString(),
                            RatingByOrg = reader["RATING_BY_ORG"]?.ToString(),
                            RatingOrgName = reader["RATING_ORG_NAME"]?.ToString()
                        } : null,

                        // Iç siyahilari (Array-l?ri) yigmaq üçün inisializasiya
                        Collaterals = new List<Collateral>(),
                        Guarantees = new List<Guarantee>()
                    };

                    credits.Add(credit);
                    creditDict.Add(credit.Id, credit); // Tez axtaris üçün lüg?t? ?lav? edirik
                }
            }

            // ====================================================================
            // 2. BORCALANLARI OXUYURUQ
            // ====================================================================
            using (var reader = ((OracleRefCursor)cmd.Parameters["P_BORROWERS_CUR"].Value).GetDataReader())
            {
                while (await reader.ReadAsync())
                {
                    var creditId = Convert.ToInt64(reader["CREDIT_ID"]);

                    //var tt = Convert.ToDecimal(reader["INC_FCY_NET_INCOME"]);
                    var borrower = new Borrower
                    {
                        CreditId = creditId,
                        Cif = reader["CIF"]?.ToString(),
                        BalanceAccountNo = reader["BALANCE_ACCOUNT_NO"]?.ToString(),
                        IsCoBorrower = reader["IS_CO_BORROWER"] != DBNull.Value && Convert.ToInt32(reader["IS_CO_BORROWER"]) == 1,
                        Name = reader["NAME"]?.ToString(),
                        DocId = reader["DOC_ID"]?.ToString(),
                        DateOfBirth = reader["DATE_OF_BIRTH"] != DBNull.Value ? Convert.ToDateTime(reader["DATE_OF_BIRTH"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                        PlaceOfBirth = reader["PLACE_OF_BIRTH"]?.ToString(),
                        CountryCode = reader["COUNTRY_CODE"]?.ToString(),
                        PinCode = reader["PIN_CODE"]?.ToString(),
                        IsBrwDiedOrDisappeared = reader["IS_BRW_DIED_DISAPPEARED"] != DBNull.Value && Convert.ToInt32(reader["IS_BRW_DIED_DISAPPEARED"]) == 1,
                        BankruptcyStatus = reader["BANKRUPTCY_STATUS"] != DBNull.Value && Convert.ToInt32(reader["BANKRUPTCY_STATUS"]) == 1,
                        BankCardCode = reader["BANK_CARD_CODE"] == DBNull.Value ? null : reader["BANK_CARD_CODE"].ToString(),
                        WorkAreaCode = reader["WORK_AREA_CODE"] == DBNull.Value ? null : reader["WORK_AREA_CODE"]?.ToString(),
                        GeneralManager = reader["GENERAL_MANAGER"] == DBNull.Value ? null : reader["GENERAL_MANAGER"]?.ToString(),
                        BusinessCategory = reader["BUSINESS_CATEGORY"] == DBNull.Value ? null : reader["BUSINESS_CATEGORY"]?.ToString(),
                        IsOpLost = reader["IS_OP_LOST"] != DBNull.Value && Convert.ToInt32(reader["IS_OP_LOST"]) == 1,

                        // T?k Obyektl?r
                        Income = reader["INC_NET_INCOME"] != DBNull.Value ? new Income
                        {
                            NetIncome = Convert.ToDecimal(reader["INC_NET_INCOME"]),
                            BgnCoefficient = reader["INC_BGN_COEFFICIENT"] != DBNull.Value ? Convert.ToDecimal(reader["INC_BGN_COEFFICIENT"]) : 0,
                            BgnOverdue = reader["INC_BGN_OVERDUE"] != DBNull.Value ? Convert.ToInt32(reader["INC_BGN_OVERDUE"]) : 0,
                            IncomeForeignCurrency = reader["INC_FCY_NET_INCOME"] != DBNull.Value && Convert.ToDecimal(reader["INC_FCY_NET_INCOME"]) != 0 ? new IncomeForeignCurrency
                            {
                                NetIncomeForeignCurrency = Convert.ToDecimal(reader["INC_FCY_NET_INCOME"]),
                                CurrencyCode = reader["INC_FCY_CURRENCY_CODE"]?.ToString()
                            } : null
                        } : null,

                        WorkPlace = reader["WORK_STATUS"] != DBNull.Value ? new WorkPlace
                        {
                            Name = reader["WORK_NAME"] != DBNull.Value ? reader["WORK_NAME"].ToString() : null,
                            Tin = reader["WORK_TIN"] != DBNull.Value ? reader["WORK_TIN"].ToString() : null,
                            WorkStatus = reader["WORK_STATUS"]?.ToString()
                        } : null,

                        Group = reader["GROUP_CODE"] != DBNull.Value ? new Group
                        {
                            Code = reader["GROUP_CODE"]?.ToString(),
                            Name = reader["GROUP_NAME"]?.ToString()
                        } : null,

                        GroupForBank = reader["GROUP_BANK_CODE"] != DBNull.Value ? new GroupForBank
                        {
                            Code = reader["GROUP_BANK_CODE"]?.ToString(),
                            InsolvencyStatus = reader["GROUP_BANK_INSOLVENCY_STAT"]?.ToString()
                        } : null,

                        // Iç-iç? Siyahilar (Array-l?r)
                        //RelationWithBanks = new List<RelationWithBank>(),
                        Shareholders = new List<Shareholder>(),
                        FinanceReports = new List<FinanceReport>(),
                        ForeignEcoActivities = new List<ForeignEcoActivity>()
                    };

                    borrowers.Add(borrower);

                    if (creditDict.TryGetValue(creditId, out var parentCredit))
                    {
                        parentCredit.Borrower = borrower;
                    }
                }
            }

            // 3A. AIDIYY?TI S?XSL?R (RelationWithBanks)
            using (var reader = ((OracleRefCursor)cmd.Parameters["P_RELATIONS_CUR"].Value).GetDataReader())
            {
                while (await reader.ReadAsync())
                {
                    var creditId = Convert.ToInt64(reader["CREDIT_ID"]);

                    var relation = new RelationWithBank
                    {
                        IsRelatedBank = reader["IS_RELATED_BANK"] != DBNull.Value && Convert.ToInt32(reader["IS_RELATED_BANK"]) == 1,
                        RelatedName = reader["RELATED_NAME"] == DBNull.Value ? null : reader["RELATED_NAME"]?.ToString(),
                        RelatedPinCode = reader["RELATED_PIN_CODE"] == DBNull.Value ? null : reader["RELATED_PIN_CODE"]?.ToString(),
                        RelatedId = reader["RELATED_ID"] == DBNull.Value ? null : reader["RELATED_ID"]?.ToString(),
                        RelatedDateOfBirth = reader["RELATED_DATE_OF_BIRTH"] != DBNull.Value ? Convert.ToDateTime(reader["RELATED_DATE_OF_BIRTH"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                        RelationCode = reader["RELATION_CODE"] == DBNull.Value ? null : reader["RELATION_CODE"]?.ToString()
                    };

                    if (creditDict.TryGetValue(creditId, out var parentCredit) && parentCredit.Borrower != null)
                    {
                        if (parentCredit.Borrower.RelationWithBanks == null)
                        {
                            parentCredit.Borrower.RelationWithBanks = new List<RelationWithBank>();
                        }

                        parentCredit.Borrower.RelationWithBanks.Add(relation);
                    }
                }
            }

            // 3B. S?HMDARLAR V? T?SISÇIL?R (Shareholders)
            using (var reader = ((OracleRefCursor)cmd.Parameters["P_SHAREHOLDERS_CUR"].Value).GetDataReader())
            {
                while (await reader.ReadAsync())
                {
                    var creditId = Convert.ToInt64(reader["CREDIT_ID"]);

                    var shareholder = new Shareholder
                    {
                        Name = reader["NAME"]?.ToString(),
                        PinCode = reader["PIN_CODE"]?.ToString(),
                        Id = reader["DOC_ID"]?.ToString(), // JSON-da sah? 'id' adlanir
                        DateOfBirth = reader["DATE_OF_BIRTH"] != DBNull.Value ? Convert.ToDateTime(reader["DATE_OF_BIRTH"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null
                    };

                    if (creditDict.TryGetValue(creditId, out var parentCredit) && parentCredit.Borrower != null)
                    {
                        parentCredit.Borrower.Shareholders.Add(shareholder);
                    }
                }
            }

            // 3C. MALIYY? HESABATLARI (FinanceReports)
            using (var reader = ((OracleRefCursor)cmd.Parameters["P_FINANCE_REPORTS_CUR"].Value).GetDataReader())
            {
                while (await reader.ReadAsync())
                {
                    var creditId = Convert.ToInt64(reader["CREDIT_ID"]);

                    var report = new FinanceReport
                    {
                        ReportingDate = reader["REPORTING_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["REPORTING_DATE"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                        ReportType = reader["REPORT_TYPE"]?.ToString(),
                        Revenue = reader["REVENUE"] != DBNull.Value ? Convert.ToDecimal(reader["REVENUE"]) : 0,
                        Ebitda = reader["EBITDA"] != DBNull.Value ? Convert.ToDecimal(reader["EBITDA"]) : 0,
                        Ebit = reader["EBIT"] != DBNull.Value ? Convert.ToDecimal(reader["EBIT"]) : 0,
                        InterestExpences = reader["INTEREST_EXPENCES"] != DBNull.Value ? Convert.ToDecimal(reader["INTEREST_EXPENCES"]) : 0,
                        NetProfit = reader["NET_PROFIT"] != DBNull.Value ? Convert.ToDecimal(reader["NET_PROFIT"]) : 0,
                        TotalAssets = reader["TOTAL_ASSETS"] != DBNull.Value ? Convert.ToDecimal(reader["TOTAL_ASSETS"]) : 0,
                        TotalAssetsForeignCurrency = reader["TOTAL_ASSETS_FCY"] != DBNull.Value ? Convert.ToDecimal(reader["TOTAL_ASSETS_FCY"]) : 0,
                        TotalLiabilitiesForeignCurrency = reader["TOTAL_LIABILITIES_FCY"] != DBNull.Value ? Convert.ToDecimal(reader["TOTAL_LIABILITIES_FCY"]) : 0,
                        InterestExpenceLiabilities = reader["INTEREST_EXPENCE_LIAB"] != DBNull.Value ? Convert.ToDecimal(reader["INTEREST_EXPENCE_LIAB"]) : 0,
                        TotalLiabilities = reader["TOTAL_LIABILITIES"] != DBNull.Value ? Convert.ToDecimal(reader["TOTAL_LIABILITIES"]) : 0,
                        ForeignInterestExpenceLiabilities = reader["FOREIGN_INT_EXPENCE_LIAB"] != DBNull.Value ? Convert.ToDecimal(reader["FOREIGN_INT_EXPENCE_LIAB"]) : 0,
                        TotalCapital = reader["TOTAL_CAPITAL"] != DBNull.Value ? Convert.ToDecimal(reader["TOTAL_CAPITAL"]) : 0,
                        InsolvencyStatus = reader["INSOLVENCY_STATUS"] != DBNull.Value && Convert.ToInt32(reader["INSOLVENCY_STATUS"]) == 1 ? true : false
                    };

                    if (creditDict.TryGetValue(creditId, out var parentCredit) && parentCredit.Borrower != null)
                    {
                        if (parentCredit.Borrower.FinanceReports == null)
                        {
                            parentCredit.Borrower.FinanceReports = new List<FinanceReport>();
                        }
                        parentCredit.Borrower.FinanceReports.Add(report);
                    }
                }
            }

            // 3D. XARICI IQTISADI F?ALIYY?T (ForeignEcoActivities)
            using (var reader = ((Oracle.ManagedDataAccess.Types.OracleRefCursor)cmd.Parameters["P_FOREIGN_ECO_CUR"].Value).GetDataReader())
            {
                while (await reader.ReadAsync())
                {
                    var creditId = Convert.ToInt64(reader["CREDIT_ID"]);

                    var eco = new ForeignEcoActivity
                    {
                        IsForeignEcoOp = reader["IS_FOREIGN_ECO_OP"] != DBNull.Value && Convert.ToInt32(reader["IS_FOREIGN_ECO_OP"]) == 1,
                        ImportExportCode = reader["IMPORT_EXPORT_CODE"] == DBNull.Value ? null : reader["IMPORT_EXPORT_CODE"]?.ToString()
                    };

                    if (creditDict.TryGetValue(creditId, out var parentCredit) && parentCredit.Borrower != null)
                    {
                        parentCredit.Borrower.ForeignEcoActivities.Add(eco);
                    }
                }
            }

            // ====================================================================
            // 4. GIROVLAR
            // ====================================================================
            using (var reader = ((Oracle.ManagedDataAccess.Types.OracleRefCursor)cmd.Parameters["P_COLLATERALS_CUR"].Value).GetDataReader())
            {
                while (await reader.ReadAsync())
                {
                    var creditId = Convert.ToInt64(reader["CREDIT_ID"]);

                    var collateral = new Collateral
                    {
                        CreditId = creditId,

                        CollateralTypeCode = reader["COLLATERAL_TYPE_CODE"]?.ToString(),
                        AnyInfoToDisting = reader["ANY_INFO_TO_DISTING"]?.ToString(),
                        CollateralValue = reader["COLLATERAL_VALUE"] != DBNull.Value ? Convert.ToDecimal(reader["COLLATERAL_VALUE"]) : 0,
                        MarketValue = reader["MARKET_VALUE"] != DBNull.Value ? Convert.ToDecimal(reader["MARKET_VALUE"]) : 0,
                        RegistryAgency = reader["REGISTRY_AGENCY"] == DBNull.Value ? null : reader["REGISTRY_AGENCY"]?.ToString(),
                        RegistryDate = reader["REGISTRY_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["REGISTRY_DATE"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                        RegistryNo = reader["REGISTRY_NO"] == DBNull.Value ? null : reader["REGISTRY_NO"]?.ToString(),
                        CollateralSubTypeCode = reader["COLLATERAL_SUB_TYPE"] == DBNull.Value || string.IsNullOrWhiteSpace(reader["COLLATERAL_SUB_TYPE"]?.ToString()) ? null : reader["COLLATERAL_SUB_TYPE"]?.ToString(),
                        CollateralGroupCode = reader["COLLATERAL_GROUP_CODE"] == DBNull.Value || string.IsNullOrWhiteSpace(reader["COLLATERAL_GROUP_CODE"]?.ToString()) ? null : reader["COLLATERAL_GROUP_CODE"]?.ToString(),

                        Evaluation = new Evaluation
                        {
                            EvaluationDateByBank = reader["EVAL_DATE_BY_BANK"] != DBNull.Value ? Convert.ToDateTime(reader["EVAL_DATE_BY_BANK"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                            EvaluationDateByExtevaluator = reader["EVAL_DATE_BY_EXT"] != DBNull.Value ? Convert.ToDateTime(reader["EVAL_DATE_BY_EXT"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                            ExternalEvaluatorName = reader["EVAL_EXT_NAME"] == DBNull.Value ? null : reader["EVAL_EXT_NAME"]?.ToString()
                        },

                        Insurance = new Insurance
                        {
                            InsuranceCompName = reader["INS_COMP_NAME"] != DBNull.Value ? reader["INS_COMP_NAME"].ToString() : null,
                            InsuranceCompTIN = reader["INS_COMP_TIN"] != DBNull.Value ? reader["INS_COMP_TIN"].ToString() : null,
                            InsuranceEndDate = reader["INS_END_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["INS_END_DATE"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                            CollateralInsurance = reader["INS_COLLATERAL_INS"] != DBNull.Value && Convert.ToInt32(reader["INS_COLLATERAL_INS"]) == 1
                        }
                    };

                    collaterals.Add(collateral);

                    if (creditDict.TryGetValue(creditId, out var parentCredit))
                    {
                        parentCredit.Collaterals.Add(collateral);
                    }
                }
            }

            // ====================================================================
            // 5. ZAMINL?RI OXUYURUQ
            // ====================================================================
            using (var reader = ((Oracle.ManagedDataAccess.Types.OracleRefCursor)cmd.Parameters["P_GUARANTEES_CUR"].Value).GetDataReader())
            {
                while (await reader.ReadAsync())
                {
                    var creditId = Convert.ToInt64(reader["CREDIT_ID"]);

                    var guarantee = new Guarantee
                    {
                        CreditId = creditId,

                        Name = reader["NAME"]?.ToString(),
                        DocId = reader["DOC_ID"]?.ToString(),
                        PinCode = reader["PIN_CODE"]?.ToString(),
                        CountryCode = reader["COUNTRY_CODE"]?.ToString(),
                        DateOfBirth = reader["DATE_OF_BIRTH"] != DBNull.Value ? Convert.ToDateTime(reader["DATE_OF_BIRTH"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                        PlaceOfBirth = reader["PLACE_OF_BIRTH"]?.ToString()
                    };

                    guarantees.Add(guarantee);

                    if (creditDict.TryGetValue(creditId, out var parentCredit))
                    {
                        parentCredit.Guarantees.Add(guarantee);
                    }
                }
            }

            return (credits, borrowers, collaterals, guarantees);
        }
    }
}
