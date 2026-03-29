import { Swiper, SwiperSlide } from "swiper/react";
import { Box, Button, Stack, IconButton, Typography } from "@mui/material";
import { useRef, useState, useEffect } from "react";
import "swiper/css";
import { ChevronLeft, ChevronRight } from "lucide-react";

import { useAccounts } from "../hooks/useAccounts";
import { ConfirmCreateAccountDialog } from "../features/account/components/ConfirmCreateAccountDialog";
import { AccountCard } from "../features/account/components/AccountCard";
import { TransferDialog } from "../features/transaction/components/TransferDialog";
import { DepositWithdrawDialog } from "../features/transaction/components/DepositWithdrawDialog";
import { NoExistingAccounts } from "../features/account/components/NoExistingAccounts";
import { TransactionTypeEnum } from "../api/transactionService";

export default function AccountsPage() {
  const { accountsList, loading, error, createAccount, fetchAccounts } =
    useAccounts();

  const swiperRef = useRef<any>(null);
  const [createAccountOpen, setCreateAccountOpen] = useState(false);
  const [transferOpen, setTransferOpen] = useState(false);
  const [depositWithdrawOpen, setDepositWithdrawOpen] = useState(false);
  const [transactionType, setTransactionType] = useState<TransactionTypeEnum>(
    TransactionTypeEnum.Deposit,
  );
  const [activeAccountId, setActiveAccountId] = useState<string | null>(null);

  useEffect(() => {
    fetchAccounts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleCreateAccount = async () => {
    createAccount();
    setCreateAccountOpen(false);
  };

  const handleDepositWithdrawClick = (transactionType: TransactionTypeEnum) => {
    setTransactionType(transactionType);
    setDepositWithdrawOpen(true);
  };

  return (
    <Box sx={{ px: 4, py: 3 }}>
      {/* Add card button */}
      <Button
        variant="contained"
        size="large"
        sx={{
          mb: 4,
          maxWidth: 300,
          background: "linear-gradient(135deg, #0f172a, #020617)",
        }}
        onClick={() => setCreateAccountOpen(true)}
      >
        + Add card
      </Button>

      <ConfirmCreateAccountDialog
        open={createAccountOpen}
        onConfirm={handleCreateAccount}
        onCancel={() => setCreateAccountOpen(false)}
        message="Are you sure want to create new account?"
      />

      {loading && <Typography>Loading...</Typography>}
      {error && <Typography color="error">{error}</Typography>}

      {!loading && !error && accountsList.length > 0 && (
        <Stack
          direction="row"
          alignItems="center"
          spacing={2}
          sx={{
            width: "100%",
            overflow: "hidden",
          }}
        >
          <IconButton onClick={() => swiperRef.current?.slidePrev()}>
            <ChevronLeft size={24} />
          </IconButton>

          <Box
            sx={{
              width: "100%",
              maxWidth: "100%",
              overflow: "hidden",
            }}
          >
            <Swiper
              onSwiper={(swiper) => {
                swiperRef.current = swiper;
                const firstAccount = accountsList[swiper.activeIndex];
                setActiveAccountId(firstAccount.id);
              }}
              slidesPerView={1}
              spaceBetween={24}
              style={{ width: "100%" }}
              onSlideChange={(swiper) => {
                const account = accountsList[swiper.activeIndex];
                setActiveAccountId(account.id);
              }}
            >
              {accountsList.map((account) => (
                <SwiperSlide key={account.id}>
                  <AccountCard id={account.id} balance={account.balance} />
                </SwiperSlide>
              ))}
            </Swiper>
          </Box>

          <IconButton onClick={() => swiperRef.current?.slideNext()}>
            <ChevronRight size={24} />
          </IconButton>
        </Stack>
      )}

      {!loading && !error && accountsList.length === 0 && (
        <Box
          sx={{
            width: "100%",
            maxWidth: "100%",
            overflow: "hidden",
          }}
        >
          <NoExistingAccounts />
        </Box>
      )}
      <Button
        variant="contained"
        size="large"
        sx={{
          mt: 4,
          mb: 4,
          maxWidth: 300,
          background: "linear-gradient(135deg, #0f172a, #020617)",
        }}
        onClick={() => setTransferOpen(true)}
      >
        Transfer
      </Button>

      <Button
        variant="contained"
        size="large"
        sx={{
          mt: 4,
          mb: 4,
          ml: 4,
          maxWidth: 300,
          background: "linear-gradient(135deg, #0f172a, #020617)",
        }}
        onClick={() => handleDepositWithdrawClick(TransactionTypeEnum.Deposit)}
      >
        Deposit
      </Button>

      <Button
        variant="contained"
        size="large"
        sx={{
          mt: 4,
          mb: 4,
          ml: 4,
          maxWidth: 300,
          background: "linear-gradient(135deg, #0f172a, #020617)",
        }}
        onClick={() => handleDepositWithdrawClick(TransactionTypeEnum.Withdraw)}
      >
        Withdraw
      </Button>

      {activeAccountId && (
        <TransferDialog
          open={transferOpen}
          onClose={() => setTransferOpen(false)}
          accounts={accountsList}
          fromAccountId={activeAccountId}
        />
      )}

      {activeAccountId && (
        <DepositWithdrawDialog
          open={depositWithdrawOpen}
          onClose={() => setDepositWithdrawOpen(false)}
          accounts={accountsList}
          fromAccountId={activeAccountId}
          transactionType={transactionType}
        />
      )}
    </Box>
  );
}
