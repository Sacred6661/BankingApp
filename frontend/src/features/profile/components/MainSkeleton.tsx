import {
  Box,
  Card,
  Typography,
  Grid,
  Skeleton,
  CardContent,
} from "@mui/material";

const MainSkeleton = () => (
  <Box>
    <Card sx={{ mb: 3 }}>
      <CardContent>
        <Typography variant="h6" mb={2}>
          Main
        </Typography>
        <Grid container spacing={2}>
          <Grid size={{ xs: 12 }}>
            <Skeleton variant="rectangular" height={40} />
          </Grid>
          <Grid size={{ xs: 6 }}>
            <Skeleton variant="rectangular" height={40} />
          </Grid>
          <Grid size={{ xs: 6 }}>
            <Skeleton variant="rectangular" height={40} />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <Skeleton variant="rectangular" height={40} />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <Skeleton variant="rectangular" height={40} />
          </Grid>
        </Grid>
      </CardContent>
    </Card>
  </Box>
);

export default MainSkeleton;
