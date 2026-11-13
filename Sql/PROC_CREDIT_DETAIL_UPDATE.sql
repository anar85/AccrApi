-- =============================================================================
-- UI kredit redaktəsi üçün Oracle prosedurları
-- Package: MKR.PKG_ACCR_SERVICE
-- Mövcud PROC_GET_BATCH_DATA kursor sütunları ilə eyni olmalıdır.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1) Tövsiyə olunan override cədvəli (əgər yoxdursa)
-- -----------------------------------------------------------------------------
/*
CREATE TABLE MKR.ACCR_CREDIT_OVERRIDE (
    BATCH_ID      NUMBER        NOT NULL,
    CREDIT_ID     NUMBER        NOT NULL,
    CREDIT_JSON   CLOB          NOT NULL,
    UPDATED_AT    DATE          DEFAULT SYSDATE NOT NULL,
    UPDATED_BY    VARCHAR2(100),
    CONSTRAINT PK_ACCR_CREDIT_OVERRIDE PRIMARY KEY (BATCH_ID, CREDIT_ID)
);
*/

-- -----------------------------------------------------------------------------
-- 2) PROC_GET_CREDIT_DATA
-- Batch-dəki tək kreditin tam qrafikini qaytarır.
-- Əgər override varsa P_OVERRIDE_JSON doldurulur; C# onu üstün tutur.
-- Kursorlar PROC_GET_BATCH_DATA ilə eyni sütun setinə sahib olmalıdır
-- (yalnız bir CREDIT_ID üçün filter).
-- -----------------------------------------------------------------------------
/*
PROCEDURE PROC_GET_CREDIT_DATA (
    P_BATCH_ID          IN  NUMBER,
    P_CREDIT_ID         IN  NUMBER,
    P_FOUND             OUT NUMBER,
    P_STATUS            OUT NUMBER,
    P_STATUS_NAME       OUT VARCHAR2,
    P_NUM_DOG           OUT VARCHAR2,
    P_ACCOUNT_NO        OUT VARCHAR2,
    P_OVERRIDE_JSON     OUT CLOB,
    P_CREDITS_CUR       OUT SYS_REFCURSOR,
    P_BORROWERS_CUR     OUT SYS_REFCURSOR,
    P_COLLATERALS_CUR   OUT SYS_REFCURSOR,
    P_GUARANTEES_CUR    OUT SYS_REFCURSOR,
    P_RELATIONS_CUR     OUT SYS_REFCURSOR,
    P_SHAREHOLDERS_CUR  OUT SYS_REFCURSOR,
    P_FINANCE_REPORTS_CUR OUT SYS_REFCURSOR,
    P_FOREIGN_ECO_CUR   OUT SYS_REFCURSOR
);
*/

-- -----------------------------------------------------------------------------
-- 3) PROC_UPDATE_CREDIT_DATA
-- UI-dan gələn tam Credit JSON-unu saxlayır.
-- Minimal implementasiya: ACCR_CREDIT_OVERRIDE-ə MERGE.
-- -----------------------------------------------------------------------------
/*
PROCEDURE PROC_UPDATE_CREDIT_DATA (
    P_BATCH_ID      IN  NUMBER,
    P_CREDIT_ID     IN  NUMBER,
    P_CREDIT_JSON   IN  CLOB,
    P_UPDATED       OUT NUMBER,
    P_MESSAGE       OUT VARCHAR2
);
*/

-- -----------------------------------------------------------------------------
-- 4) PROC_GET_BATCH_OVERRIDES
-- Batch göndərilərkən UI redaktələrinin CBAR JSON-una düşməsi üçün.
-- Cursor sütunları: CREDIT_ID, CREDIT_JSON
-- -----------------------------------------------------------------------------
/*
PROCEDURE PROC_GET_BATCH_OVERRIDES (
    P_BATCH_ID       IN  NUMBER,
    P_OVERRIDES_CUR  OUT SYS_REFCURSOR
);
*/

-- Qeyd: Bu fayl yalnız kontrakt/şablon kimidir.
-- Real PL/SQL PROC_GET_BATCH_DATA mənbəyinə əsasən package body-yə əlavə edilməlidir.
-- C# artıq bu imzaları çağırır.
-- Minimal DBA yolu: yalnız OVERRIDE cədvəli + UPDATE/GET_CREDIT_DATA(override)/GET_BATCH_OVERRIDES.
