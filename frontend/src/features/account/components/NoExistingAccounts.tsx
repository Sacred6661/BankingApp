import type { FC } from "react";
import { Card, CardContent, Typography } from "@mui/material";

export const NoExistingAccounts: FC = () => {
  return (
    <Card
      sx={{
        height: 180,
        borderRadius: 4,
        //p: 2.5,
        position: "relative",
        overflow: "hidden",

        border: 1,
        borderColor: "grey",
      }}
    >
      <CardContent
        sx={{
          height: "100%",
          p: 2.5,
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
        }}
      >
        <Typography
          variant="h5"
          sx={{
            letterSpacing: 1,
            textAlign: "center",
          }}
        >
          You have no accounts yet. Click "+ Add Card" button above to add one.
        </Typography>
      </CardContent>
    </Card>
  );
};
