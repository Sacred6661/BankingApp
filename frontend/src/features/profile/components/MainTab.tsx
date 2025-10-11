import {
  Box,
  Card,
  CardContent,
  Grid,
  TextField,
  Typography,
  FormControlLabel,
  Switch,
  Button,
} from "@mui/material";
import { VirtualizedAutocomplete } from "../../../components/ui/VirtualizedAutocomplete";

interface Props {
  formik: any;
  languages: { id: number; name: string }[];
  timezones: { id: number; name: string }[];
}

export default function MainTab({ formik, languages, timezones }: Props) {
  return (
    <Box>
      {/* Main info */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" mb={2}>
            Main
          </Typography>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <TextField
                label="Email"
                fullWidth
                disabled
                InputProps={{
                  readOnly: true,
                }}
                {...formik.getFieldProps("email")}
              />
            </Grid>
            <Grid size={{ xs: 6 }}>
              <TextField
                label="First Name"
                fullWidth
                {...formik.getFieldProps("firstName")}
                error={!!formik.touched.firstName && !!formik.errors.firstName}
                helperText={formik.touched.firstName && formik.errors.firstName}
              />
            </Grid>
            <Grid size={{ xs: 6 }}>
              <TextField
                label="Last Name"
                fullWidth
                {...formik.getFieldProps("lastName")}
                error={!!formik.touched.lastName && !!formik.errors.lastName}
                helperText={formik.touched.lastName && formik.errors.lastName}
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField
                label="Avatar URL"
                fullWidth
                {...formik.getFieldProps("avatarUrl")}
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <TextField
                label="Birthday"
                type="date"
                fullWidth
                name="birthday"
                value={
                  formik.values.birthday
                    ? formik.values.birthday.split("T")[0]
                    : ""
                }
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                InputLabelProps={{ shrink: true }}
                error={!!formik.touched.birthday && !!formik.errors.birthday}
                helperText={formik.touched.birthday && formik.errors.birthday}
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Settings */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" mb={2}>
            Settings
          </Typography>
          <Grid container spacing={2}>
            <Grid size={{ xs: 6 }}>
              <VirtualizedAutocomplete
                value={
                  languages.find(
                    (l) => l.id === formik.values.settings?.languageId
                  ) || null
                }
                options={languages}
                getOptionLabel={(l) => l.name}
                onChange={(val) =>
                  formik.setFieldValue("settings.languageId", val?.id)
                }
                label="Language"
                error={!!formik.errors.settings?.languageId}
                helperText={formik.errors.settings?.languageId}
              />
            </Grid>
            <Grid size={{ xs: 6 }}>
              <VirtualizedAutocomplete
                value={
                  timezones.find(
                    (l) => l.id === formik.values.settings?.timezoneId
                  ) || null
                }
                options={timezones}
                getOptionLabel={(l) => l.name}
                onChange={(val) =>
                  formik.setFieldValue("settings.timezoneId", val?.id)
                }
                label="Timezone"
                error={!!formik.errors.settings?.timezoneId}
                helperText={formik.errors.settings?.timezoneId}
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formik.values.settings.notificationsEnabled}
                    onChange={(e) =>
                      formik.setFieldValue(
                        "settings.notificationsEnabled",
                        e.target.checked
                      )
                    }
                  />
                }
                label="Notifications Enabled"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Button variant="contained" fullWidth type="submit" sx={{ mt: 2 }}>
        Save
      </Button>
    </Box>
  );
}
