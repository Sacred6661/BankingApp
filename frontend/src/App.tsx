import { Routes, Route } from "react-router-dom";
import { PrivateRoute } from "./components/PrivateRoute";
import { PublicRoute } from "./components/PublicRoute";
import { AuthProvider } from "./providers/AuthProvider";

import HomePage from "./pages/HomePage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import ProfilePage from "./pages/ProfilePage";
import { Container } from "@mui/material";
import { Toaster } from "react-hot-toast";

function App() {
  return (
    <Container maxWidth="lg">
      <AuthProvider>
        <Routes>
          {/* Public */}
          <Route element={<PublicRoute />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
          </Route>

          {/* Private */}
          <Route element={<PrivateRoute />}>
            <Route path="/" element={<HomePage />} />
            <Route path="/profile" element={<ProfilePage />} />
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
