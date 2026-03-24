import type { FC } from "react";
import { Box, Card, CardContent, Typography } from "@mui/material";
import "swiper/css";
import { formatMoneyWithCurrency } from "../utils/FormatMoneyHelper";

interface AccountCardProps {
  id: string;
  balance: number;
}

export const AccountCard: FC<AccountCardProps> = ({ id, balance }) => {
  return (
    <Card
      sx={{
        height: 180,
        borderRadius: 4,
        p: 2.5,
        position: "relative",
        overflow: "hidden",

        background: "linear-gradient(135deg, #0f172a, #020617)",
        color: "white",

        boxShadow: "0 16px 40px rgba(0,0,0,0.45)",
        transition: "transform 0.25s ease, box-shadow 0.25s ease",

        "&:hover": {
          transform: "translateY(-4px)",
          boxShadow: "0 24px 50px rgba(0,0,0,0.55)",
        },

        // декоративне світло
        "&::after": {
          content: '""',
          position: "absolute",
          top: -40,
          right: -40,
          width: 140,
          height: 140,
          background:
            "radial-gradient(circle, rgba(255,255,255,0.15), transparent 60%)",
        },
      }}
    >
      <CardContent
        sx={{
          height: "100%",
          p: 0,
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
        }}
      >
        {/* Верх */}
        <Box>
          <Typography
            variant="overline"
            sx={{ opacity: 0.6, letterSpacing: 1.5 }}
          >
            Account
          </Typography>

          <Typography
            sx={{
              fontSize: 13,
              opacity: 0.8,
              wordBreak: "break-all",
            }}
          >
            {id}
          </Typography>
        </Box>

        {/* Низ */}
        <Box>
          <Typography
            variant="overline"
            sx={{ opacity: 0.6, letterSpacing: 1.5 }}
          >
            Balance
          </Typography>

          <Typography variant="h4" fontWeight={600} sx={{ lineHeight: 1.2 }}>
            {formatMoneyWithCurrency(balance)}
          </Typography>
        </Box>
      </CardContent>
    </Card>
  );
};
