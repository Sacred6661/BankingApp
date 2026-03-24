import {
  Box,
  CircularProgress,
  DialogContent,
  DialogActions,
  Typography,
} from "@mui/material";

const TransferProcessing = () => {
  return (
    <>
      <DialogContent>
        <Box textAlign="center">
          <CircularProgress />
          <Typography>Processing your information...</Typography>
        </Box>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}></DialogActions>
    </>
  );
};

export default TransferProcessing;
