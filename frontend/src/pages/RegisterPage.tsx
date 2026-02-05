import { useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "../app/store";
import { register, clearError } from "../features/auth/authSlice";
import {
  Container,
  Box,
  TextField,
  Button,
  Typography,
  Alert,
  CircularProgress,
} from "@mui/material";
import { Link } from "react-router-dom";
import { getErrorMessage } from "../utils/errorHelpers";
import PasswordRequirements from "../features/auth/components/PasswordRequirements";
import { isPasswordValid } from "../features/auth/utils/passwordRules";

export default function RegisterPage() {
  const dispatch = useDispatch<AppDispatch>();
  const { loading, error } = useSelector((state: RootState) => state.auth);

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const passwordsMatch = password === confirmPassword;
  const passwordIsValid = isPasswordValid(password);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!passwordIsValid || !passwordsMatch) return;

    dispatch(register({ email, password }));
  };

  return (
    <Container maxWidth="sm">
      <Box
        display="flex"
        flexDirection="column"
        justifyContent="center"
        alignItems="center"
        minHeight="100vh"
      >
        <Box
          component="form"
          onSubmit={handleSubmit}
          sx={{
            width: "100%",
            p: 4,
            borderRadius: 3,
            boxShadow: 3,
            bgcolor: "background.paper",
          }}
        >
          <Typography variant="h4" component="h1" textAlign="center" mb={3}>
            Register
          </Typography>

          <TextField
            label="Email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            fullWidth
            margin="normal"
            required
          />

          <TextField
            label="Password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            fullWidth
            margin="normal"
            required
          />

          <PasswordRequirements password={password} />

          <TextField
            label="Confirm Password"
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            fullWidth
            margin="normal"
            required
            error={!passwordsMatch && confirmPassword.length > 0}
            helperText={
              !passwordsMatch && confirmPassword.length > 0
                ? "Passwords do not match"
                : ""
            }
          />

          <Box mt={3} position="relative">
            <Button
              type="submit"
              variant="contained"
              color="primary"
              fullWidth
              disabled={loading || !passwordsMatch || !passwordIsValid}
            >
              Register
            </Button>

            {loading && (
              <CircularProgress
                size={24}
                sx={{
                  color: "primary.main",
                  position: "absolute",
                  top: "50%",
                  left: "50%",
                  marginTop: "-12px",
                  marginLeft: "-12px",
                }}
              />
            )}
          </Box>

          {error && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {getErrorMessage(error)}
            </Alert>
          )}

          <Typography variant="body2" textAlign="center" mt={3}>
            Already have an account?{" "}
            <Link
              to="/login"
              style={{ textDecoration: "none", color: "#1976d2" }}
              onClick={() => dispatch(clearError())}
            >
              Login here
            </Link>
          </Typography>
        </Box>
      </Box>
    </Container>
  );
}
