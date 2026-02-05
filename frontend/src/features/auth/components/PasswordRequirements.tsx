import { Box, Typography } from "@mui/material";
import { passwordRules, validatePassword } from "../utils/passwordRules";

interface Props {
  password: string;
}

export default function PasswordRequirements({ password }: Props) {
  if (!password) return null;

  const validation = validatePassword(password);

  return (
    <Box mt={1}>
      {Object.entries(passwordRules).map(([key, rule]) => {
        const isValid = validation[key as keyof typeof validation];

        return (
          <Typography
            key={key}
            variant="body2"
            sx={{
              color: isValid ? "success.main" : "text.secondary",
            }}
          >
            {isValid ? "✔" : "✖"} {rule.label}
          </Typography>
        );
      })}
    </Box>
  );
}
