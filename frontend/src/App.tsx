import { Routes, Route } from "react-router-dom";
import { PrivateRoute } from "./components/PrivateRoute";
import { UnauthenticatedRoute } from "./components/UnauthenticatedRoute";
import { AuthProvider } from "./providers/AuthProvider";
import { ProfileRedirect } from "./components/ProfileRedirect";

import HomePage from "./pages/HomePage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import ProfilePage from "./pages/ProfilePage";
import AccountsPage from "./pages/AccountsPage";
import { Container } from "@mui/material";
import { Toaster } from "react-hot-toast";

function App() {
  return (
    <Container maxWidth="lg">
      <AuthProvider>
        <ProfileRedirect />
        <Routes>
          {/* Public */}
          <Route element={<UnauthenticatedRoute />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
          </Route>

          {/* Private */}
          <Route element={<PrivateRoute />}>
            <Route path="/" element={<HomePage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/accounts" element={<AccountsPage />} />
          </Route>
        </Routes>
      </AuthProvider>
      <Toaster
        position="bottom-right"
        toastOptions={{
          duration: 4000,
        }}
      />
    </Container>
  );
}

export default App;
